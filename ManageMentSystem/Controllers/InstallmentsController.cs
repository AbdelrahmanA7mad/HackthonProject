using ManageMentSystem.Data;
using ManageMentSystem.Helpers;
using ManageMentSystem.Models;
using ManageMentSystem.Services;
using ManageMentSystem.Services.CustomerServices;
using ManageMentSystem.Services.InstallmentServices;
using ManageMentSystem.Services.UserInvoice;
using ManageMentSystem.Services.WhatsAppServices;

using ManageMentSystem.Services.UserServices;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace ManageMentSystem.Controllers
{
    [Authorize]

    public class InstallmentsController : Controller
    {
        private readonly IInstallmentService _installmentService;
        private readonly PdfService _pdfService;
        private readonly ICustomerService _customerService;
        private readonly AppDbContext _context;
        private readonly IUserInvoice _userInvoice;
        private readonly IUserService _userService;
        private readonly IWhatsAppService _whatsAppService;

        public InstallmentsController(IUserInvoice userInvoice ,IInstallmentService installmentService, PdfService pdfService, ICustomerService customerService, AppDbContext context, IUserService userService, IWhatsAppService whatsAppService)
        {
            _installmentService = installmentService;
            _pdfService = pdfService;
            _customerService = customerService;
            _context = context;
            _userInvoice = userInvoice;
            _userService = userService;
            _whatsAppService = whatsAppService;
        }

        // GET: Installments
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string searchTerm = "", string sortBy = "SequenceNumber", string sortOrder = "desc")
        {
            // Update installment statuses first
            await _installmentService.UpdateOverdueInstallmentsAsync();
            
            var paginatedInstallments = await _installmentService.GetPaginatedInstallmentsAsync(page, pageSize, searchTerm, sortBy, sortOrder);
            ViewBag.Customers = await _installmentService.GetAllCustomersAsync();
            ViewBag.Products = await _installmentService.GetAllProductsAsync();
            ViewBag.summary = await _installmentService.GetInstallmentSummaryAsync();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            
            return View(paginatedInstallments);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaginatedInstallments(int page = 1, int pageSize = 20, string searchTerm = "", string sortBy = "SequenceNumber", string sortOrder = "desc")
        {
        // API endpoint for AJAX pagination
            try
            {
                // Update installment statuses first (only on first page to avoid performance issues)
                if (page == 1)
                {
                    await _installmentService.UpdateOverdueInstallmentsAsync();
                }
                
                var result = await _installmentService.GetPaginatedInstallmentsAsync(page, pageSize, searchTerm, sortBy, sortOrder);
                return Json(new
                {
                    success = true,
                    installments = result.Installments.Select(i => new
                    {
                        id = i.Id,
                        sequenceNumber = i.SequenceNumber,
                        customerName = i.Customer?.FullName,
                        customerId = i.CustomerId,
                        totalAmount = i.TotalAmount,
                        downPayment = i.DownPayment,
                        monthlyPayment = i.MonthlyPayment,
                        numberOfMonths = i.NumberOfMonths,
                        interestRate = i.InterestRate,
                        interestAmount = i.InterestAmount,
                        totalWithInterest = i.TotalWithInterest,
                        startDate = i.StartDate.ToString("dd/MM/yyyy"),
                        status = i.Status,
                        productsCount = i.InstallmentItems?.Count() ?? 0
                    }),
                    currentPage = result.CurrentPage,
                    totalPages = result.TotalPages,
                    totalCount = result.TotalCount,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء تحميل البيانات" });
            }
        }

        // GET: Installments/Details/5
        public async Task<IActionResult> Details(int? id, string? message = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            var installmentDetails = await _installmentService.GetInstallmentDetailsWithMonthlyPaymentsAsync(id.Value);
            if (installmentDetails == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(message))
            {
                ViewBag.SuccessMessage = message;
            }

            return View(installmentDetails);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _installmentService.GetAllCustomersAsync();
            ViewBag.Products = await _installmentService.GetAllProductsAsync();
            ViewBag.PaymentMethods = await _context.PaymentMethodOptions
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.SortOrder)
                .ToListAsync();
            return View(new CreateInstallmentViewModel());
        }

        // POST: Installments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInstallmentViewModel model)
        {
            // Check if creating new customer
            if (model.CustomerId == 0)
            {
                if (!string.IsNullOrEmpty(model.NewCustomerName) && !string.IsNullOrEmpty(model.NewCustomerPhone))
                {
                    // Use the new AddOrUpdateCustomerAsync method
                    var customerViewModel = new CreateCustomerViewModel
                    {
                        FullName = model.NewCustomerName,
                        PhoneNumber = model.NewCustomerPhone,
                        Address = model.NewCustomerAddress
                    };
                    var (success, customer, message) = await _customerService.AddOrUpdateCustomerAsync(customerViewModel);
                    if (success && customer != null)
                    {
                        model.CustomerId = customer.Id;
                    }
                    else
                    {
                        ModelState.AddModelError("", message ?? "فشل في إنشاء العميل الجديد. يرجى المحاولة مرة أخرى.");
                    }
                }
                else
                {
                    ModelState.AddModelError("CustomerId", "يرجى اختيار عميل أو إدخال بيانات العميل الجديد");
                }
            }

            // Additional validation for customer
            if (model.CustomerId == 0 && (string.IsNullOrEmpty(model.NewCustomerName) || string.IsNullOrEmpty(model.NewCustomerPhone)))
            {
                ModelState.AddModelError("CustomerId", "يرجى اختيار عميل أو إدخال بيانات العميل الجديد");
            }

            if (ModelState.IsValid)
            {
                var installment = await _installmentService.AddInstallmentAsync(model);
                if (installment != null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "تم إضافة التقسيط بنجاح" });
                    }
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "فشل في إنشاء التقسيط. يرجى المحاولة مرة أخرى.");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ في البيانات",
                    errors = errors
                });
            }

            ViewBag.Customers = await _installmentService.GetAllCustomersAsync();
            ViewBag.Products = await _installmentService.GetAllProductsAsync();
            ViewBag.PaymentMethods = await _context.PaymentMethodOptions
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.SortOrder)
                .ToListAsync();
            return View(model);
        }


        // POST: Installments/UpdateMonths
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMonths(int id, int numberOfMonths)
        {
            if (numberOfMonths < 1)
            {
                return Json(new { success = false, message = "عدد الشهور يجب أن يكون 1 على الأقل" });
            }

            try
            {
                var result = await _installmentService.RescheduleInstallmentAsync(id, numberOfMonths);
                if (result)
                {
                    return Json(new { success = true, message = "تم إعادة جدولة القسط بنجاح" });
                }
                return Json(new { success = false, message = "فشل في تحديث القسط" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }


        // GET: Installments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var installment = await _installmentService.GetInstallmentByIdAsync(id.Value);
            if (installment == null)
            {
                return NotFound();
            }

            return View(installment);
        }

        // POST: Installments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // الحصول على تفاصيل القسط قبل الحذف
                var installment = await _installmentService.GetInstallmentByIdAsync(id);
                if (installment == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "القسط غير موجود" });
                    }
                    TempData["Error"] = "القسط غير موجود";
                    return RedirectToAction("Index");
                }

                var productNames = installment.InstallmentItems?.Select(ii => ii.Product?.Name).Where(n => !string.IsNullOrEmpty(n)) ?? new List<string>();
                var productName = productNames.Any() ? string.Join(", ", productNames) : "منتجات متعددة";

                // دائماً حذف مع إلغاء العملية بالكامل (restoreInventory = true)
                var result = await _installmentService.DeleteInstallmentAsync(id);
                
                if (result)
                {
                    var message = $"تم حذف القسط بنجاح وإرجاع المنتجات للمخزون وإلغاء جميع العمليات المرتبطة";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = message });
                    }
                    
                    TempData["Success"] = message;
                    return RedirectToAction("Index");
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "فشل في حذف القسط" });
                    }
                    TempData["Error"] = "فشل في حذف القسط";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"حدث خطأ أثناء حذف القسط: {ex.Message}" });
                }
                TempData["Error"] = $"حدث خطأ أثناء حذف القسط: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Installments/GetProducts
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _installmentService.GetAllProductsAsync();
            var productList = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                description = p.Description ?? "",
                price = p.SalePrice,
                quantity = p.Quantity
            }).ToList();

            return Json(productList);
        }

        // GET: Installments/GetCustomers
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _installmentService.GetAllCustomersAsync();
            var customerList = customers.Select(c => new
            {
                id = c.Id,
                fullName = c.FullName
            }).ToList();

            return Json(customerList);
        }

        [HttpGet]
        public async Task<IActionResult> PrintInvoice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var installmentDetails = await _installmentService.GetInstallmentDetailsWithMonthlyPaymentsAsync(id.Value);
            if (installmentDetails == null)
            {
                return NotFound();
            }

            installmentDetails.CompanySettings = await _userInvoice.GetInvoiceAsync();

            return View("PrintInvoice", installmentDetails);
        }


        [HttpGet]
        public async Task<IActionResult> SendWhatsapp(int id)
        {
            try
            {
                var installment = await _installmentService.GetInstallmentByIdAsync(id);
                if (installment == null || installment.Customer == null)
                    return Json(new { success = false, message = "لم يتم العثور على عملية التقسيط أو العميل" });

                var phone = installment.Customer.PhoneNumber?.Trim();
                if (string.IsNullOrEmpty(phone))
                    return Json(new { success = false, message = "رقم هاتف العميل مطلوب" });

                var userId = await _userService.GetRootUserIdAsync();
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "لم يتم العثور على معرف المستخدم" });

                var status = await _whatsAppService.GetSessionStatusAsync(userId);
                if (!status.SessionExists || !status.IsConnected)
                    return Json(new { success = false, message = "يجب عليك توصيل حساب واتساب أولاً", needsQR = true });

                var message = $"مرحبًا {installment.Customer.FullName}، هذه فاتورة الاقساط من شركتنا. شكرًا لثقتك بنا!";

                // Generate the invoice PDF on the fly
                var installmentDetails = await _installmentService.GetInstallmentDetailsWithMonthlyPaymentsAsync(id);
                if (installmentDetails == null)
                    return Json(new { success = false, message = "لم يتم العثور على بيانات الفاتورة" });

                installmentDetails.CompanySettings = await _userInvoice.GetInvoiceAsync();
                var pdfBytes = await _pdfService.RenderViewToPdfAsync("Installments/PrintInvoice", installmentDetails);
                var fileName = $"installment_{id}_{Guid.NewGuid()}.pdf";

                var result = await _whatsAppService.SendInvoicePdfAsync(userId, phone, installment.Customer.FullName ?? "عزيزي العميل", message, pdfBytes, fileName);

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = $"{result.ErrorMessage}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطأ غير متوقع: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SendOverdueAlert(int id)
        {
            try
            {
                var installment = await _installmentService.GetInstallmentByIdAsync(id);
                if (installment == null || installment.Customer == null)
                    return Json(new { success = false, message = "لم يتم العثور على عملية التقسيط أو العميل" });

                var phone = installment.Customer.PhoneNumber?.Trim();
                if (string.IsNullOrEmpty(phone))
                    return Json(new { success = false, message = "رقم هاتف العميل مطلوب" });

                if (installment.Status != "متأخر")
                    return Json(new { success = false, message = "هذا القسط ليس متأخراً" });

                var userId = await _userService.GetRootUserIdAsync();
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "لم يتم العثور على معرف المستخدم" });

                var status = await _whatsAppService.GetSessionStatusAsync(userId);
                if (!status.SessionExists || !status.IsConnected)
                    return Json(new { success = false, message = "يجب عليك توصيل حساب واتساب أولاً", needsQR = true });

                // إنشاء رسالة تنبيه مخصصة
                var overdueDays = (DateTime.Now - installment.StartDate.AddMonths(installment.NumberOfMonths)).Days;
                var message = GenerateOverdueMessage(installment, overdueDays);

                var result = await _whatsAppService.SendMessageAsync(userId, phone, message);

                if (result.Success)
                {
                    return Json(new { success = true, message = "تم إرسال تنبيه التأخير بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = $"فشل في الإرسال: {result.ErrorMessage}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطأ غير متوقع: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendBulkOverdueAlerts()
        {
            try
            {
                var userId = await _userService.GetRootUserIdAsync();
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "لم يتم العثور على معرف المستخدم" });

                var status = await _whatsAppService.GetSessionStatusAsync(userId);
                if (!status.SessionExists || !status.IsConnected)
                    return Json(new { success = false, message = "يجب عليك توصيل حساب واتساب أولاً", needsQR = true });

                var overdueInstallments = await _installmentService.GetOverdueInstallmentsAsync();
                if (!overdueInstallments.Any())
                    return Json(new { success = false, message = "لا توجد أقساط متأخرة" });

                var sentCount = 0;
                var failedCount = 0;
                var errors = new List<string>();

                foreach (var installment in overdueInstallments)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(installment.Customer?.PhoneNumber))
                        {
                            failedCount++;
                            errors.Add($"العميل {installment.Customer?.FullName} لا يوجد له رقم هاتف");
                            continue;
                        }

                        var phone = installment.Customer.PhoneNumber.Trim();
                        var overdueDays = (DateTime.Now - installment.StartDate.AddMonths(installment.NumberOfMonths)).Days;
                        var message = GenerateOverdueMessage(installment, overdueDays);

                        var result = await _whatsAppService.SendMessageAsync(userId, phone, message);

                        if (result.Success)
                        {
                            sentCount++;
                        }
                        else
                        {
                            failedCount++;
                            errors.Add($"فشل في إرسال تنبيه للعميل {installment.Customer.FullName}: {result.ErrorMessage}");
                        }

                        // تأخير قصير بين الرسائل لتجنب الحظر
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        errors.Add($"خطأ في إرسال تنبيه للعميل {installment.Customer?.FullName}: {ex.Message}");
                    }
                }

                var messageAll = $"تم إرسال {sentCount} تنبيه بنجاح";
                if (failedCount > 0)
                {
                    messageAll += $". فشل في إرسال {failedCount} تنبيه";
                }

                return Json(new { 
                    success = true, 
                    message = messageAll,
                    sentCount = sentCount,
                    failedCount = failedCount,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطأ غير متوقع: {ex.Message}" });
            }
        }




        private string GenerateOverdueMessage(Installment installment, int overdueDays)
        {
            var customerName = installment.Customer.FullName;
            var productNames = string.Join("، ", installment.InstallmentItems?.Select(ii => ii.Product?.Name ?? "غير محدد") ?? new List<string>());
            var remainingAmount = installment.RemainingAmount.ToString("N0");
            var monthlyPayment = installment.MonthlyPayment.ToString("N0");

            var message = $"تنبيه مهم - تأخير في الدفع\n\n";
            message += $"مرحباً {customerName}،\n\n";
            message += $"نود تذكيرك بأن موعد دفع القسط الشهري قد حان.\n\n";
            message += $"تفاصيل التقسيط:\n";
            message += $"• المنتجات: {productNames}\n";
            message += $"• المبلغ المتبقي: {remainingAmount} جنيه\n";
            message += $"• الدفعة الشهرية المطلوبة: {monthlyPayment} جنيه\n";
            message += $"يرجى التواصل معنا فوراً لتسوية المدفوعات المتأخرة.\n\n";
            message += $"شكراً لتفهمك وتعاونك معنا";

            return message;
        }


        // POST: Installments/AddExtraMonth/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExtraMonth(int id)
        {
            try
            {
                var success = await _installmentService.AddExtraMonthAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "تم إضافة الشهر الإضافي بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في إضافة الشهر الإضافي أو القسط يحتوي على شهر إضافي بالفعل" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطأ: {ex.Message}" });
            }
        }

        // POST: Installments/RemoveExtraMonth/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveExtraMonth(int id)
        {
            try
            {
                var success = await _installmentService.RemoveExtraMonthAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "تم إزالة الشهر الإضافي بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "فشل في إزالة الشهر الإضافي أو القسط لا يحتوي على شهر إضافي" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطأ: {ex.Message}" });
            }
        }

        // GET: Installments/GetExtraMonthAmount/5
        [HttpGet]
        public async Task<IActionResult> GetExtraMonthAmount(int id)
        {
            try
            {
                var amount = await _installmentService.CalculateExtraMonthAmountAsync(id);
                return Json(new { success = true, amount = amount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطأ: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInstallmentData(int id)
        {
            try
            {
                var installment = await _installmentService.GetInstallmentByIdAsync(id);
                if (installment == null)
                    return Json(new { success = false, message = "القسط غير موجود" });

                return Json(new
                {
                    success = true,
                    id = installment.Id,
                    numberOfMonths = installment.NumberOfMonths
                });
            }
            catch (Exception ex)
            {
                 return Json(new { success = false, message = $"خطأ في الخادم: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOverdueInstallmentsList()
        {
            try
            {
                var overdueInstallments = await _installmentService.GetOverdueInstallmentsAsync();
                
                // Filter out invalid ones immediately to give accurate count
                var validList = overdueInstallments
                    .Where(i => !string.IsNullOrEmpty(i.Customer?.PhoneNumber))
                    .Select(i => new { 
                        id = i.Id, 
                        customerName = i.Customer?.FullName ?? "عميل",
                        phone = i.Customer?.PhoneNumber
                    })
                    .ToList();

                return Json(new { success = true, list = validList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطأ غير متوقع: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOverdueInstallmentsCount()
        {
            try
            {
                var overdueInstallments = await _installmentService.GetOverdueInstallmentsAsync();
                return Json(new { 
                    success = true, 
                    count = overdueInstallments.Count() 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطأ غير متوقع: {ex.Message}" });
            }
        }
    }
}