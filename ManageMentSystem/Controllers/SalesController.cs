using ManageMentSystem.Data;
using ManageMentSystem.Helpers;
using ManageMentSystem.Models;
using ManageMentSystem.Services;
using ManageMentSystem.Services.CustomerAccountServices;
using ManageMentSystem.Services.CustomerServices;
using ManageMentSystem.Services.SalesServices;
using ManageMentSystem.Services.UserInvoice;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.Services.WhatsAppServices;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
 
namespace ManageMentSystem.Controllers
{

    [Authorize]
    public class SalesController : Controller
    {
        private readonly ISalesService _salesService;
        private readonly PdfService _pdfService;
        private readonly ICustomerService _customerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly IUserInvoice _userInvoice;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ICustomerAccountService _customerAccountService;

        public SalesController(ICustomerAccountService customerAccountService, ISalesService salesService, PdfService pdfService, ICustomerService customerService, UserManager<ApplicationUser> userManager, IUserService userService , IUserInvoice userInvoice, IWhatsAppService whatsAppService)
        {
            _salesService = salesService;
            _pdfService = pdfService;
            _customerService = customerService;
            _userManager = userManager;
            _userService = userService;
            _userInvoice = userInvoice;
            _whatsAppService = whatsAppService;
            _customerAccountService = customerAccountService;

        }

        // GET: Sales
        public async Task<IActionResult> Index(int page = 1, string searchTerm = "", DateTime? fromDate = null, DateTime? toDate = null, ManageMentSystem.Models.SalePaymentType? paymentType = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var pageSize = 20;
            var (sales, totalCount, totalPages) = await _salesService.GetSalesPaginatedAsync(page, pageSize, searchTerm, paymentType, fromDate, toDate);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.PaymentType = paymentType;
            ViewBag.CurrentUser = currentUser;
            return View(sales);
        }

        // GET: Sales/Details/5
        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _salesService.GetSaleByIdAsync(id.Value);
            if (sale == null)
            {
                return NotFound();
            }

            // If AJAX request, return JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new {
                    id = sale.Id,
                    customerName = sale.Customer?.FullName ?? "غير محدد",
                    totalAmount = sale.TotalAmount.ToString("F2"),
                    saleDate = sale.SaleDate.ToString("yyyy-MM-dd"),
                    paymentType = sale.PaymentType.ToString(),
                    paidAmount = sale.PaidAmount.ToString("F2")
                });
            }

            return View(sale);
        }

      
        public async Task<IActionResult> QuickSale()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id ?? string.Empty;

            ViewBag.Customers = await _salesService.GetAllCustomersAsync();
            ViewBag.PaymentMethods = await _salesService.GetPaymentMethodsAsync();
            ViewBag.CurrentUser = currentUser;

            
            return View(new CreateSaleViewModel());
        }

        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        [HttpPost]
        public async Task<IActionResult> Create(CreateSaleViewModel model)
        {
            // تحقق من وجود منتجات
            if (model.SaleItems == null || !model.SaleItems.Any())
            {
                ModelState.AddModelError("", "يجب إضافة منتج واحد على الأقل.");
            }

            // إضافة الوقت الحالي للتاريخ
            model.SaleDate = model.SaleDate.Date.Add(DateTime.Now.TimeOfDay);

            // إنشاء عميل جديد لو معطى
            if (!model.CustomerId.HasValue || model.CustomerId == 0)
            {
                if (!string.IsNullOrEmpty(model.NewCustomerName) && !string.IsNullOrEmpty(model.NewCustomerPhone))
                {
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
                else if (model.PaymentType != SalePaymentType.Cash)
                {
                    // العميل مطلوب فقط للآجل/الجزئي
                    ModelState.AddModelError("CustomerId", "يجب اختيار عميل للبيع الآجل أو الجزئي");
                }
                // إذا كاش → سيتم ربطه بالعميل الافتراضي تلقائي
            }

            // دعم AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (!ModelState.IsValid)
                {
                    var errorMessage = string.Join(" | ", errors);
                    return Json(new { success = false, message = $"حدث خطأ: {errorMessage}", errors });
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var sale = await _salesService.AddSaleAsync(model, currentUser?.Id ?? string.Empty);

                    if (sale != null)
                    {
                        sale = await _salesService.GetSaleByIdAsync(sale.Id); // تحميل الـ navigation

                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new
                            {
                                success = true,
                                message = "تم إضافة البيع بنجاح",
                                saleId = sale.Id,
                                customerName = sale.Customer?.FullName ?? "غير محدد",
                                totalAmount = sale.TotalAmount.ToString("F2")
                            });
                        }

                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "فشل في إنشاء البيع. يرجى المحاولة مرة أخرى.");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "حدث خطأ غير متوقع.");
                }
            }

            // إعادة تحميل القوائم للـ View
            ViewBag.Customers = await _salesService.GetAllCustomersAsync();
            ViewBag.Products = await _salesService.GetAllProductsAsync();
            ViewBag.PaymentMethods = await _salesService.GetPaymentMethodsAsync();

            return View(model);
        }


        // GET: Sales/Edit/5
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _salesService.GetSaleByIdAsync(id.Value);
            if (sale == null)
            {
                return NotFound();
            }

            var model = new CreateSaleViewModel
            {
                CustomerId = sale.CustomerId ?? 0,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                DiscountPercentage = sale.DiscountPercentage,
                DiscountAmount = sale.DiscountAmount,
                SaleItems = sale.SaleItems.Select(si => new SaleItemViewModel
                {
                    ProductId = si.ProductId,
                    Quantity = si.Quantity,
                    UnitPrice = si.UnitPrice,
                    CustomSalePrice = si.UnitPrice,
                    PurchasePrice = si.Product?.PurchasePrice ?? 0,
                    ProductName = si.Product?.Name ?? "منتج غير معروف"
                }).ToList()
            };

            ViewBag.SaleId = id.Value;
            ViewBag.CustomerId = id.Value;
            ViewBag.Customers = await _salesService.GetAllCustomersAsync();
            ViewBag.Products = await _salesService.GetAllProductsAsync();
            ViewBag.PaymentMethods = await _salesService.GetPaymentMethodsAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    id = sale.Id,
                    customerId = sale.CustomerId,
                    customerName = sale.Customer?.FullName,
                    saleDate = sale.SaleDate.ToString("yyyy-MM-dd"),
                    totalAmount = sale.TotalAmount,
                    paidAmount = sale.PaidAmount,
                    paymentType = (int)sale.PaymentType,
                    discountPercentage = sale.DiscountPercentage,
                    discountAmount = sale.DiscountAmount,
                    saleItems = sale.SaleItems.Select(si => new
                    {
                        productId = si.ProductId,
                        productName = si.Product?.Name,
                        quantity = si.Quantity,
                        unitPrice = si.UnitPrice,
                        purchasePrice = si.Product?.PurchasePrice ?? 0,
                        totalPrice = si.UnitPrice * si.Quantity
                    }).ToList()
                });
            }

            return View(model);
        }

        // POST: Sales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, CreateSaleViewModel model)
        {
            // إضافة الوقت الحالي للتاريخ
            model.SaleDate = model.SaleDate.Date.Add(DateTime.Now.TimeOfDay);
            
            // إذا كان طلب AJAX، إرجاع JSON مع تفاصيل البيانات
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                try
                {
                    // التحقق من صحة البيانات
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();
                        
                        return Json(new { 
                            success = false, 
                            message = "أخطاء في النموذج: " + string.Join(", ", errors),
                            errors = errors
                        });
                    }

                    // التحقق من وجود SaleItems
                    if (model.SaleItems == null || !model.SaleItems.Any())
                    {
                        return Json(new { 
                            success = false, 
                            message = "لا توجد منتجات مختارة"
                        });
                    }

                    var updatedSale = await _salesService.UpdateSaleAsync(id, model);
                    if (updatedSale != null)
                    {
                        // Reload sale with navigation properties
                        var saleWithDetails = await _salesService.GetSaleByIdAsync(updatedSale.Id);
                        return Json(new { 
                            success = true, 
                            message = "تم تحديث البيع بنجاح",
                            saleId = updatedSale.Id,
                            customerName = saleWithDetails?.Customer?.FullName ?? "غير محدد",
                            totalAmount = updatedSale.TotalAmount.ToString("F2")
                        });
                    }
                    else
                    {
                        return Json(new { 
                            success = false, 
                            message = "فشل في تحديث البيع"
                        });
                    }
                }
                catch (InvalidOperationException ex)
                {
                    return Json(new { 
                        success = false, 
                        message = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { 
                        success = false, 
                        message = $"حدث خطأ أثناء تحديث البيع: {ex.Message}"
                    });
                }
            }

            // إذا كان طلب عادي (غير AJAX)
            if (ModelState.IsValid)
            {
                try
                {
                    var updatedSale = await _salesService.UpdateSaleAsync(id, model);
                    if (updatedSale != null)
                    {
                        TempData["SuccessMessage"] = "تم تحديث البيع بنجاح";
                        // إعادة التوجيه إلى StoreAccount/Index ليرى المستخدم رأس المال المحدث
                        return RedirectToAction("Index", "StoreAccount");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "فشل في تحديث البيع";
                        return RedirectToAction("Index");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"حدث خطأ أثناء تحديث البيع: {ex.Message}";
                    return RedirectToAction("Index");
                }
            }

            // إذا كان هناك أخطاء في النموذج
            var errorMessage = "يرجى تصحيح الأخطاء في النموذج";
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction("Index");
        }

        // GET: Sales/Delete/5
        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _salesService.GetSaleByIdAsync(id.Value);
            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _salesService.DeleteSaleAsync(id);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var message = result ? "تم حذف البيع بالكامل بنجاح" : "فشل في حذف البيع";
                    return Json(new { success = result, message = message });
                }
                
                if (result)
                {
                    TempData["SuccessMessage"] = "تم حذف البيع بالكامل بنجاح";
                }
                else
                {
                    TempData["ErrorMessage"] = "فشل في حذف البيع";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"حدث خطأ أثناء الحذف: {ex.Message}" });
                }
                
                // إعادة توجيه مع رسالة خطأ
                TempData["ErrorMessage"] = $"حدث خطأ أثناء الحذف: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Sales/GetProducts
        [HttpGet]
        
        public async Task<IActionResult> GetProducts()
        {
            var products = await _salesService.GetAllProductsAsync();
            var productList = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                description = p.Description ?? "",
                price = p.SalePrice,
                purchasePrice = p.PurchasePrice,
                quantity = p.Quantity,
                barcode = p.Barcode ?? ""
            }).ToList();

            return Json(productList);
        }

        // GET: Sales/GetCustomers
        [HttpGet]
        
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _salesService.GetAllCustomersAsync();
            var customerList = customers.Select(c => new
            {
                id = c.Id,
                fullName = c.FullName
            }).ToList();

            return Json(customerList);
        }

        // GET: Sales/PrintInvoice/5
        
        public async Task<IActionResult> PrintInvoice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var sale = await _salesService.GetSaleByIdAsync(id.Value);
            if (sale == null)
            {
                return NotFound();
            }

            var companySettings = await _userInvoice.GetInvoiceAsync();
            var viewModel = new SaleInvoiceViewModel
            {
                Sale = sale,
                CompanySettings = companySettings
            };

            return View("PrintInvoice", viewModel);
        }

        // GET: Sales/ReceiptInvoice/5
        
        public async Task<IActionResult> ReceiptInvoice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var sale = await _salesService.GetSaleByIdAsync(id.Value);
            if (sale == null)
            {
                return NotFound();
            }

            var companySettings = await _userInvoice.GetInvoiceAsync();
            var viewModel = new SaleInvoiceViewModel
            {
                Sale = sale,
                CompanySettings = companySettings
            };

            return View("ReceiptInvoice", viewModel);
        }



        [HttpGet]
        
        public async Task<IActionResult> SendWhatsapp(int id)
        {
            try
            {
                var sale = await _salesService.GetSaleByIdAsync(id);
                if (sale == null || sale.Customer == null)
                    return Json(new { success = false, message = "لم يتم العثور على عملية البيع أو العميل" });

                var phone = sale.Customer.PhoneNumber?.Trim();
                if (string.IsNullOrEmpty(phone))
                    return Json(new { success = false, message = "رقم هاتف العميل مطلوب" });

                // ✅ الحصول على userId من المستخدم الحالي (استخدام RootUserId لضمان التوافق مع جلسة الواتساب الخاصة بالفرع/الشركة)
                var userId = await _userService.GetRootUserIdAsync();
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "❌ لم يتم العثور على معرف المستخدم" });

                // ✅ التحقق من حالة جلسة الواتساب أولاً
                var status = await _whatsAppService.GetSessionStatusAsync(userId);

                if (!status.SessionExists || !status.IsConnected)
                {
                    return Json(new
                    {
                        success = false,
                        message = "❌ يجب عليك توصيل حساب واتساب أولاً",
                        needsQR = true
                    });
                }

                var message = $"مرحبًا {sale.Customer.FullName}، هذه فاتورتك من شركتنا. شكرًا لثقتك بنا!";

                // Generate the invoice PDF on the fly
                var companySettings = await _userInvoice.GetInvoiceAsync();
                var invoiceViewModel = new SaleInvoiceViewModel
                {
                    Sale = sale,
                    CompanySettings = companySettings
                };

                var pdfBytes = await _pdfService.RenderViewToPdfAsync("Sales/PrintInvoice", invoiceViewModel);
                var fileName = $"invoice_{id}_{Guid.NewGuid()}.pdf";

                // ✅ إرسال الفاتورة عبر الخدمة
                var result = await _whatsAppService.SendInvoicePdfAsync(
                    userId, 
                    phone, 
                    sale.Customer.FullName ?? "عزيزي العميل", 
                    message, 
                    pdfBytes, 
                    fileName);

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = $"❌ {result.ErrorMessage}"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"❌ خطأ غير متوقع: {ex.Message}" });
            }
        }


        // GET: Sales/ReturnFromSale/5
        
        public async Task<IActionResult> ReturnFromSale(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _salesService.GetSaleByIdAsync(id.Value);
            if (sale == null)
            {
                return NotFound();
            }

            var model = new CreateReturnFromSaleViewModel
            {
                SaleId = sale.Id,
                CustomerId = sale.CustomerId,
                CustomerName = sale.Customer?.FullName ?? "غير محدد",
                ReturnDate = DateTime.Now,
                SaleItems = sale.SaleItems.Select(si => new SaleItemForReturnViewModel
                {
                    SaleItemId = si.Id,
                    ProductId = si.ProductId ?? 0,
                    ProductName = si.Product?.Name ?? "غير محدد",
                    Barcode = si.Product?.Barcode,
                    OriginalQuantity = si.Quantity,
                    ReturnQuantity = 0,
                    UnitPrice = si.UnitPrice,
                    IsSelected = false
                }).ToList()
            };

            ViewBag.Customers = await _customerService.GetAllCustomersAsync();
            return View(model);
        }

        // GET: Sales/UnpaidSales
        
        public async Task<IActionResult> UnpaidSales(DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20)
        {
            var (sales, totalCount, totalPages, totalUnpaidAmount) = await _salesService.GetUnpaidSalesAsync(fromDate, toDate, page, pageSize);

            ViewBag.PaymentMethods = await _salesService.GetPaymentMethodsAsync();

            var model = new UnpaidSalesViewModel
            {
                Sales = sales,
                TotalUnpaidAmount = totalUnpaidAmount,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                FromDate = fromDate,
                ToDate = toDate
            };

            return View(model);
        }

        // POST: Sales/AddPaymentForSale
        [HttpPost]
        
        public async Task<IActionResult> AddPaymentForSale(CustomerPaymentInputViewModel model)
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

    }








} 
