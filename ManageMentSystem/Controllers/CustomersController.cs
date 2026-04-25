using ManageMentSystem.Data;
using ManageMentSystem.Helpers;
using ManageMentSystem.Models;
using ManageMentSystem.Services.CustomerAccountServices;
using ManageMentSystem.Services.CustomerServices;
using ManageMentSystem.Services.ExcelExportServices;
using ManageMentSystem.Services.UserInvoice;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.Services.WhatsAppServices;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
//
namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerAccountService _customerAccountService;
        private readonly AppDbContext _context;
        private readonly ManageMentSystem.Services.PdfService _pdfService;
        private readonly IUserService _userService;
        private readonly IUserInvoice _userInvoice;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IExcelExportService _excelExportService;


        public CustomersController(ICustomerService customerService, ICustomerAccountService customerAccountService, AppDbContext context, ManageMentSystem.Services.PdfService pdfService , IUserService userService  ,IUserInvoice userInvoice, IWhatsAppService whatsAppService, IExcelExportService excelExportService)
        {
            _customerService = customerService;
            _customerAccountService = customerAccountService;
            _context = context;
            _pdfService = pdfService;
            _userService = userService;
            _userInvoice = userInvoice;
            _whatsAppService = whatsAppService;
            _excelExportService = excelExportService;
        }

        // GET: Customers
        public async Task<IActionResult> Index(int page = 1, string searchTerm = "")
        {
            var pageSize = 20;
            var (customers, totalCount, totalPages) = await _customerService.GetCustomersPaginatedAsync(page, pageSize, searchTerm);
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchTerm = searchTerm;
            
            return View(customers);
        }
        [HttpGet]
        public async Task<IActionResult> GetCustomerBalance(int customerId)
        {
            try
            {
                var balance = await _customerAccountService.GetCustomerBalanceAsync(customerId);
                return Json(new { balance = balance });
            }
            catch (Exception ex)
            {
                return Json(new { balance = 0, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStoreBalance()
        {
            try
            {
                var storeAccountService = HttpContext.RequestServices.GetRequiredService<ManageMentSystem.Services.StoreAccountServices.IStoreAccountService>();
                var balance = await storeAccountService.GetCashBalanceAsync();
                return Json(new { balance = balance });
            }
            catch (Exception ex)
            {
                return Json(new { balance = 0, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPayment(CustomerPaymentInputViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "بيانات غير صحيحة" });
            }
            try
            {
                var payment = await _customerAccountService.AddPaymentAsync(model);
                return Json(new { success = true, message = "تم تسجيل الدفعة بنجاح", paymentId = payment.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePayment(int id)
        {
            try
            {
                var ok = await _customerAccountService.DeletePaymentAsync(id);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = ok, message = ok ? "تم حذف الدفعة" : "لم يتم العثور على الدفعة" });
                }
                TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "تم حذف الدفعة" : "لم يتم العثور على الدفعة";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = ex.Message });
                }
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Statement(int id, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20)
        {
            // التحقق من ملكية العميل للمستخدم الحالي
            var customer = await _customerService.GetCustomerByIdAsync(id);
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (customer == null || customer.TenantId != tenantId)
            {
                return NotFound();
            }

            var statement = await _customerAccountService.GetCustomerStatementAsync(id, fromDate, toDate, page, pageSize);
            ViewBag.PaymentMethods = await _context.PaymentMethodOptions
                .Where(pm => pm.IsActive && pm.TenantId == tenantId)
                .OrderBy(pm => pm.SortOrder)
                .ToListAsync();
            
            ViewBag.CustomerName = customer.FullName;
            ViewBag.CustomerPhone = customer.PhoneNumber ?? "-";
            return View(statement);
        }

        [HttpGet]
        public async Task<IActionResult> FullAccount(int id, int page = 1, int pageSize = 20)
        {
            // التحقق من ملكية العميل للمستخدم الحالي
            var customer = await _customerService.GetCustomerByIdAsync(id);
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (customer == null || customer.TenantId != tenantId)
            {
                return NotFound();
            }

            var full = await _customerAccountService.GetCustomerFullAccountAsync(id, page, pageSize);
            var companySettings = await _userInvoice.GetInvoiceAsync();
            ApplyCompanySettings(full, companySettings);
            return View(full);
        }

        [HttpGet]
        public async Task<IActionResult> SendWhatsappStatement(int id, bool full = false)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                    return Json(new { success = false, message = "العميل غير موجود" });

                var phone = customer.PhoneNumber?.Trim();
                if (string.IsNullOrEmpty(phone))
                    return Json(new { success = false, message = "رقم هاتف العميل مطلوب" });

                var userId = await _userService.GetRootUserIdAsync();
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "❌ لم يتم العثور على معرف المستخدم" });

                var status = await _whatsAppService.GetSessionStatusAsync(userId);
                if (!status.SessionExists || !status.IsConnected)
                    return Json(new { success = false, message = "❌ يجب عليك توصيل حساب واتساب أولاً", needsQR = true });

                // حساب الإجماليات للرسالة
                var totalSalesAmount = await _context.Sales
                    .Where(s => s.CustomerId == id)
                    .SumAsync(s => s.TotalAmount);
                
                
                var netSalesAmount = totalSalesAmount;
                var totalPaid = await _context.Sales
                    .Where(s => s.CustomerId == id)
                    .SumAsync(s => s.PaidAmount);
                var balance = Math.Max(0, netSalesAmount - totalPaid);

                var message = $"مرحبًا {customer.FullName}،\n" +
                             $"📊هذاكشف حسابك \n" +
                             $"شكرًا لثقتك بنا! 🙏";

                // Build model and render PDF
                var model = await _customerAccountService.GetCustomerFullAccountAsync(id, 1, 100000);
                var companySettings = await _userInvoice.GetInvoiceAsync();
                ApplyCompanySettings(model, companySettings);
                // استخدم View للطباعة لا يعتمد على UrlHelper
                var pdfBytes = await _pdfService.RenderViewToPdfAsync("Customers/FullAccountPrint", model);
                var fileName = $"customer_statement_{id}_{Guid.NewGuid()}.pdf";

                var result = await _whatsAppService.SendInvoicePdfAsync(userId, phone, customer.FullName ?? "عزيزي العميل", message, pdfBytes, fileName);

                if (result.Success)
                {
                    return Json(new { success = true, message = "✅ تم إرسال كشف الحساب عبر واتساب" });
                }
                else
                {
                    return Json(new { success = false, message = $"❌ فشل في الإرسال: {result.ErrorMessage}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ خطأ غير متوقع: {ex.Message}" });
            }
        }

        private static void ApplyCompanySettings(CustomerFullAccountViewModel model, Invoice? companySettings)
        {
            if (model == null || companySettings == null)
            {
                return;
            }

            model.CompanyName = companySettings.CompanyName ?? model.CompanyName;
            model.CompanySubtitle = companySettings.CompanySubtitle;
            model.Address = companySettings.Address;
            model.FooterMessage = companySettings.FooterMessage;
            model.Website = companySettings.Website;
            model.Email = companySettings.Email;
            model.Logo = companySettings.Logo;
            model.PhoneNumbers = companySettings.PhoneNumbers?.ToList() ?? new List<string>();
        }



        // GET: Customers/Create
        public IActionResult Create()
        {
            return View(new CreateCustomerViewModel());
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = await _customerService.AddCustomerAsync(model);
                if (customer != null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "تم إضافة العميل بنجاح" });
                    }
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "فشل في إنشاء العميل. يرجى المحاولة مرة أخرى.");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "حدث خطأ في البيانات" });
            }

            return View(model);
        }

        // GET: Customers/SendBulkWhatsapp
        public async Task<IActionResult> SendBulkWhatsapp(int page = 1, int pageSize = 50)
        {
            // ✅ التحقق من حالة اتصال واتساب
            var userId = await _userService.GetRootUserIdAsync();

            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    var status = await _whatsAppService.GetSessionStatusAsync(userId);
                    ViewBag.IsWhatsAppConnected = status.IsConnected;
                    ViewBag.WhatsAppSessionExists = status.SessionExists;
                }
                else
                {
                    ViewBag.IsWhatsAppConnected = false;
                    ViewBag.WhatsAppSessionExists = false;
                }
            }
            catch
            {
                ViewBag.IsWhatsAppConnected = false;
                ViewBag.WhatsAppSessionExists = false;
            }

            var customers = await _customerService.GetAllCustomersAsync();
            var customersWithPhone = customers.Where(c => !string.IsNullOrEmpty(c.PhoneNumber)).ToList();

            // تطبيق الصفحات
            var totalCount = customersWithPhone.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var paginatedCustomers = customersWithPhone
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalCustomers = customers.Count;
            ViewBag.CustomersWithPhone = customersWithPhone.Count;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            return View(paginatedCustomers);
        }

        [HttpPost]
        public async Task<IActionResult> SendSingleMessage(string phone, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(message))
                {
                    return Json(new { success = false, message = "رقم الهاتف والرسالة مطلوبان" });
                }

                // ✅ الحصول على userId
                var userId = await _userService.GetRootUserIdAsync();
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "❌ لم يتم العثور على معرف المستخدم" });

                var status = await _whatsAppService.GetSessionStatusAsync(userId);
                if (!status.SessionExists || !status.IsConnected)
                {
                    return Json(new
                    {
                        success = false,
                        message = "❌ يجب عليك توصيل حساب واتساب أولاً",
                        needsConnection = true
                    });
                }

                var result = await _whatsAppService.SendMessageAsync(userId, phone, message);

                if (result.Success)
                {
                    return Json(new { success = true, message = "✅ تم الإرسال بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = $"❌ {result.ErrorMessage}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ خطأ: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendBulkWhatsapp(string message, string selectedCustomers, bool sendToAll = false, IFormFile file = null)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    return Json(new { success = false, message = "❌ الرسالة مطلوبة" });
                }

                // ✅ الحصول على userId
                var userId = await _userService.GetRootUserIdAsync();
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "❌ لم يتم العثور على معرف المستخدم" });

                var status = await _whatsAppService.GetSessionStatusAsync(userId);
                if (!status.SessionExists || !status.IsConnected)
                {
                    return Json(new
                    {
                        success = false,
                        message = "❌ يجب عليك توصيل حساب واتساب أولاً. انتقل إلى صفحة واتساب للتوصيل.",
                        needsConnection = true
                    });
                }

                var customers = new List<Models.Customer>();

                if (sendToAll)
                {
                    customers = (await _customerService.GetAllCustomersAsync())
                        .Where(c => !string.IsNullOrEmpty(c.PhoneNumber))
                        .ToList();
                }
                else if (!string.IsNullOrEmpty(selectedCustomers))
                {
                    try
                    {
                        var customerIds = selectedCustomers.Split(',')
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Select(s => int.Parse(s.Trim()))
                            .ToList();

                        customers = (await _customerService.GetAllCustomersAsync())
                            .Where(c => customerIds.Contains(c.Id) && !string.IsNullOrEmpty(c.PhoneNumber))
                            .ToList();
                    }
                    catch (FormatException)
                    {
                        return Json(new { success = false, message = "❌ تنسيق غير صحيح لمعرفات العملاء المحددين" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "❌ يرجى اختيار العملاء أو تحديد إرسال للجميع" });
                }

                if (!customers.Any())
                {
                    return Json(new { success = false, message = "❌ لا يوجد عملاء لديهم أرقام هواتف" });
                }

                // إعداد بيانات العملاء مع الأسماء
                var customerData = customers.Select(c => (object)new {
                    phone = c.PhoneNumber.Trim(),
                    name = c.FullName
                }).ToList();

                var phones = customers.Select(c => c.PhoneNumber.Trim()).ToList();

                // Generate personalized messages here in the controller where we have the type safety
                var personalizedMessages = customers.Select(c => 
                    message.Replace("{اسم_العميل}", c.FullName ?? "عميل")
                ).ToList();

                if (file != null && file.Length > 0)
                {
                    // إرسال مع ملف
                    using var fileStream = file.OpenReadStream();
                    var result = await _whatsAppService.SendBulkMessageWithFileAsync(userId, phones, customerData, personalizedMessages, fileStream, file.FileName, file.ContentType);
                    
                    if (result.Success)
                    {
                        return Json(new
                        {
                            success = true,
                            message = result.Message,
                            successCount = result.SuccessCount,
                            failCount = result.FailCount,
                            totalCount = phones.Count
                        });
                    }
                    else
                    {
                        return Json(new { success = false, message = $"❌ {result.Message}" });
                    }
                }
                else
                {
                    // إرسال نصي فقط
                    var result = await _whatsAppService.SendBulkMessageAsync(userId, phones, customerData, personalizedMessages);

                    if (result.Success)
                    {
                        return Json(new
                        {
                            success = true,
                            message = result.Message,
                            successCount = result.SuccessCount,
                            failCount = result.FailCount,
                            totalCount = phones.Count
                        });
                    }
                    else
                    {
                        return Json(new { success = false, message = $"❌ {result.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ خطأ غير متوقع: {ex.Message}" });
            }
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _customerService.GetCustomerByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }

            var model = new CreateCustomerViewModel
            {
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address
            };

            ViewBag.CustomerId = id.Value;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    id = customer.Id,
                    fullName = customer.FullName,
                    phoneNumber = customer.PhoneNumber,
                    Address = customer.Address
                });
            }

            return View(model);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var customer = await _customerService.UpdateCustomerAsync(id, model);
                    if (customer != null)
                    {
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = true, message = "تم تحديث العميل بنجاح" });
                        }
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "فشل في تحديث العميل. يرجى المحاولة مرة أخرى.");
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "حدث خطأ في البيانات" });
            }

            return View(model);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _customerService.GetCustomerByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _customerService.DeleteCustomerAsync(id);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = success, message = message });
            }
            
            if (!success)
            {
                TempData["ErrorMessage"] = message;
            }
            else
            {
                TempData["SuccessMessage"] = message;
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomerAjax([FromForm] string fullName, [FromForm] string phoneNumber, [FromForm] string Address)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Json(new { success = false, message = "يجب إدخال الاسم ورقم الهاتف" });
            }

            var model = new CreateCustomerViewModel { FullName = fullName, PhoneNumber = phoneNumber ,Address = Address};
            var customer = await _customerService.AddCustomerAsync(model);
            if (customer != null)
            {
                return Json(new { success = true, customer = new { id = customer.Id, fullName = customer.FullName } });
            }
            return Json(new { success = false, message = "فشل في إضافة العميل. حاول مرة أخرى." });
        }

        [HttpPost]
        public async Task<IActionResult> CheckCustomerPhone([FromForm] string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Json(new { exists = false, message = "يرجى إدخال رقم الهاتف" });
            }

            var existingCustomer = await _customerService.GetCustomerByPhoneAsync(phoneNumber);
            
            if (existingCustomer != null)
            {
                return Json(new { 
                    exists = true, 
                    customer = new { 
                        id = existingCustomer.Id, 
                        fullName = existingCustomer.FullName,
                        phoneNumber = existingCustomer.PhoneNumber,
                        address = existingCustomer.Address
                    },
                    message = $"العميل '{existingCustomer.FullName}' موجود بالفعل برقم الهاتف هذا"
                });
            }
            
            return Json(new { exists = false, message = "رقم الهاتف متاح" });
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateCustomerAjax([FromForm] string fullName, [FromForm] string phoneNumber, [FromForm] string address)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                return Json(new { success = false, message = "يجب إدخال الاسم ورقم الهاتف" });
            }

            var model = new CreateCustomerViewModel { FullName = fullName, PhoneNumber = phoneNumber, Address = address };
            var (success, customer, message) = await _customerService.AddOrUpdateCustomerAsync(model);
            
            if (success && customer != null)
            {
                return Json(new { 
                    success = true, 
                    customer = new { 
                        id = customer.Id, 
                        fullName = customer.FullName,
                        phoneNumber = customer.PhoneNumber,
                        address = customer.Address
                    },
                    message = message
                });
            }
            
            return Json(new { success = false, message = message });
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var customerList = customers.Select(c => new { 
                id = c.Id, 
                fullName = c.FullName,
                phoneNumber = c.PhoneNumber,
                address = c.Address
            }).ToList();
            
            return Json(customerList);
        }

        // GET: Customers/TopCustomers
        public async Task<IActionResult> TopCustomers(int page = 1, int pageSize = 20)
        {
            var customers = await _customerService.GetAllCustomersWithDetailsAsync();
            var topCustomers = customers
                .OrderByDescending(c => c.Sales?.Count ?? 0)
                .ToList();
            
            var totalCount = topCustomers.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // تطبيق الصفحات
            var paginatedCustomers = topCustomers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            ViewBag.TotalCustomers = customers.Count;
            ViewBag.TopCustomersCount = totalCount;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            
            return View(paginatedCustomers);
        }
        // GET: Customers/ExportToExcel
        public async Task<IActionResult> ExportToExcel()
        {
            var customers = await _customerService.GetAllCustomersWithDetailsAsync();

            var columns = new Dictionary<string, Func<Models.Customer, object>>
            {
                { "المعرف", c => c.Id },
                { "الاسم", c => c.FullName },
                { "رقم الهاتف", c => c.PhoneNumber },
                { "العنوان", c => c.Address },
                { "عدد المبيعات", c => c.Sales?.Count ?? 0 },
                { "تاريخ الإضافة", c => c.CreatedAt }
            };

            var fileContent = _excelExportService.ExportGeneric(customers, "العملاء", columns, "تقرير العملاء");

            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Customers_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
    }
} 