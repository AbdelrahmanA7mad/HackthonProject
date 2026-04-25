
using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.CategoryServices;
using ManageMentSystem.Services.ProductServices;
using ManageMentSystem.Services.SalesServices;
using ManageMentSystem.Services.StoreAccountServices;
using ManageMentSystem.ViewModels;
using ManageMentSystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ClosedXML.Excel;
using ManageMentSystem.Services.SystemSettings;
using Microsoft.AspNetCore.Authorization;

namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISalesService _salesService;
        private readonly AppDbContext _context;
        private readonly IStoreAccountService _storeAccountService;
        private readonly ISystemSettingsService _systemSettingsService;

        public ProductsController(IProductService productService, ICategoryService categoryService, ISalesService salesService, AppDbContext context, IStoreAccountService storeAccountService, ISystemSettingsService systemSettingsService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _salesService = salesService;
            _context = context;
            _storeAccountService = storeAccountService;
            _systemSettingsService = systemSettingsService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchByName(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var products = await _productService.GetAllProductsAsync();
            var matches = products
                .Where(p => p.Name != null && p.Name.Trim().ToLower().Contains(term.Trim().ToLower()))
                .Select(p => new { p.Id, p.Name })
                .ToList();
            return Json(matches);
        }

        [HttpGet]
        public async Task<IActionResult> CheckBarcodeUnique(string barcode, int? id = null)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return Json(true);

            var isUnique = await _productService.IsBarcodeUniqueAsync(barcode, id);
            return Json(isUnique);
        }





        [HttpGet]
        public async Task<IActionResult> SearchByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return Json(null);

            var product = await _productService.GetProductByBarcodeAsync(barcode);
            if (product == null)
                return Json(null);

            return Json(new { 
                product.Id, 
                product.Name, 
                product.Quantity, 
                product.SalePrice,
                CategoryName = product.Category?.Name
            });
        }
        // GET: Products
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? q = null, int? categoryId = null)
        {
            var allProducts = await _productService.GetAllProductsAsync();
            
            // Filter by category if provided
            if (categoryId.HasValue)
            {
                allProducts = allProducts.Where(p => p.CategoryId == categoryId.Value).ToList();
            }
            
            // Apply search query filter (name, barcode, category)
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                allProducts = allProducts.Where(p =>
                        (!string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(p.Barcode) && p.Barcode.ToLower().Contains(term)) ||
                        (p.Category != null && !string.IsNullOrEmpty(p.Category.Name) && p.Category.Name.ToLower().Contains(term))
                    )
                    .ToList();
            }
            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            
            // جلب إعدادات المخزون لاستخدامها في العرض
            var inventorySettings = await _systemSettingsService.GetInventorySettingsAsync();
            ViewBag.LowStockThreshold = inventorySettings.LowStockThreshold;
            
            // تطبيق Pagination
            var totalItems = allProducts.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;
            ViewBag.Query = q ?? string.Empty;
            ViewBag.SelectedCategoryId = categoryId;
            
            return View("AllProducts", products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create(int? categoryId)
        {
            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            
            var model = new CreateProductViewModel();
            if (categoryId.HasValue)
            {
                model.CategoryId = categoryId.Value;
            }
            
            return View(model);
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, message, product) = await _productService.AddProductWithBalanceCheckAsync(model);
                
                if (success)
                {
                    // إضافة الرصيد المتبقي للرسالة
                    var remainingBalance = await _storeAccountService.GetTotalCapitalAsync();
                    var totalCost = model.PurchasePrice * model.Quantity;
                    var enhancedMessage = $"{message}\n" +
                                        $"التكلفة الإجمالية: {totalCost:C}\n" +
                                        $"الرصيد المتبقي: {remainingBalance:C}";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = enhancedMessage });
                    }
                    TempData["SuccessMessage"] = enhancedMessage;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", message);
                    
                    // إضافة معلومات إضافية للخطأ
                    if (message.Contains("رصيد المحل غير كافي"))
                    {
                        // إنشاء عملية شراء في حساب المحل
                        var defaultPaymentMethodId = await _context.PaymentMethodOptions
                            .Where(pm => pm.IsDefault)
                            .OrderBy(pm => pm.SortOrder)
                            .Select(pm => (int?)pm.Id)
                            .FirstOrDefaultAsync() ?? 1;


                        var currentBalance = await _storeAccountService.GetCashBalanceByPaymentMethodAsync(defaultPaymentMethodId);

                        var totalCost = model.PurchasePrice * model.Quantity;
                        var additionalInfo = $"\n\nمعلومات إضافية:\n" +
                                           $"التكلفة المطلوبة: {totalCost:C}\n" +
                                           $"الرصيد الحالي: {currentBalance:C}\n" +
                                           $"الفرق المطلوب: {totalCost - currentBalance:C}";
                        
                        ModelState.AddModelError("", additionalInfo);
                    }
                    
                    // إضافة رسالة خطأ في TempData
                    TempData["ErrorMessage"] = message;
                }
            }

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            return View(model);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            var model = new CreateProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = product.Quantity,
                Description = product.Description,
                PurchasePrice = product.PurchasePrice,
                SalePrice = product.SalePrice,
                Barcode = product.Barcode,
                CategoryId = product.CategoryId
            };

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    id = product.Id,
                    name = product.Name,
                    categoryId = product.CategoryId,
                    quantity = product.Quantity,
                    description = product.Description,
                    purchasePrice = product.PurchasePrice,
                    salePrice = product.SalePrice,
                    barcode = product.Barcode
                });
            }

            return View(model);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateProductViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var (success, message, product) = await _productService.UpdateProductAsync(id, model);

                    if (success)
                    {
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = true, message = message }); // Use service message
                        }
                        TempData["SuccessMessage"] = message; // Use service message
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // Add the specific error message from the service
                        ModelState.AddModelError("", message);

                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = message });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (assuming you have logging configured)
                    // _logger.LogError(ex, "Error updating product with ID {ProductId}", id);

                    var errorMessage = "حدث خطأ غير متوقع أثناء تحديث المنتج. يرجى المحاولة مرة أخرى.";
                    ModelState.AddModelError("", errorMessage);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMessage });
                    }
                }
            }

            // Reload categories for the view in case of validation errors
            try
            {
                ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error loading categories for product edit form");
                ViewBag.Categories = new List<Category>(); // Fallback to empty list
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, CreateProductViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var (success, message, product) = await _productService.UpdateProductAsync(id, model);

                    if (success)
                    {
                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = true, message = message }); // Use service message
                        }
                        TempData["SuccessMessage"] = message; // Use service message
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // Add the specific error message from the service
                        ModelState.AddModelError("", message);

                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = message });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (assuming you have logging configured)
                    // _logger.LogError(ex, "Error updating product with ID {ProductId}", id);

                    var errorMessage = "حدث خطأ غير متوقع أثناء تحديث المنتج. يرجى المحاولة مرة أخرى.";
                    ModelState.AddModelError("", errorMessage);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMessage });
                    }
                }
            }

            // Reload categories for the view in case of validation errors
            try
            {
                ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error loading categories for product edit form");
                ViewBag.Categories = new List<Category>(); // Fallback to empty list
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            return View(model);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var (success, message) = await _productService.DeleteProductAsync(id);
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = success, message = message });
            }
            
            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Products/ByCategory/5
        public async Task<IActionResult> ByCategory(int categoryId, int page = 1, int pageSize = 20, string? q = null)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            var viewModel = new CategoryStatisticsViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategoryDescription = category.Description
            };

            var allProducts = await _productService.GetProductsByCategoryAsync(categoryId);
            
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                allProducts = allProducts
                    .Where(p =>
                        (!string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(term)) ||
                        (!string.IsNullOrEmpty(p.Barcode) && p.Barcode.ToLower().Contains(term))
                    )
                    .ToList();
            }
            
            // تطبيق Pagination على المنتجات
            var totalItems = allProducts.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            var products = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            viewModel.TotalProducts = allProducts.Sum(p => p.Quantity); // إجمالي الكمية بدلاً من عدد المنتجات
            viewModel.TotalQuantity = allProducts.Sum(p => p.Quantity);
            viewModel.TotalPurchaseValue = allProducts.Sum(p => p.PurchasePrice * p.Quantity);
            viewModel.TotalSaleValue = allProducts.Sum(p => p.SalePrice * p.Quantity);
            viewModel.PotentialProfit = viewModel.TotalSaleValue - viewModel.TotalPurchaseValue;
            viewModel.ProfitMargin = viewModel.TotalPurchaseValue > 0 ?
                (viewModel.PotentialProfit / viewModel.TotalPurchaseValue) * 100 : 0;

            viewModel.LowStockProducts = allProducts.Count(p => p.Quantity > 0 && p.Quantity < 5);
            viewModel.OutOfStockProducts = allProducts.Count(p => p.Quantity == 0);
            viewModel.HighValueProducts = allProducts.Count(p => p.SalePrice > 1000);

            var productIds = products.Select(p => p.Id).ToList();
            var allSales = await _context.SaleItems
                .Include(si => si.Sale)
                .ThenInclude(s => s.Customer)
                .Include(si => si.Product)
                .Where(si => si.ProductId.HasValue && productIds.Contains(si.ProductId.Value))
                .ToListAsync();

            var recentSales = allSales
                .OrderByDescending(si => si.Sale.SaleDate)
                .Take(20)
                .ToList();

            viewModel.TotalSalesCount = allSales.Select(si => si.SaleId).Distinct().Count();
            viewModel.TotalSalesValue = allSales.Sum(si => si.UnitPrice * si.Quantity);
            viewModel.TotalUnitsSold = allSales.Sum(si => si.Quantity);
            viewModel.TotalProfit = allSales.Sum(si => (si.UnitPrice - si.Product.PurchasePrice) * si.Quantity);

            viewModel.RecentSales = recentSales.Select(si => new SaleStatisticsViewModel
            {
                SaleId = si.Sale.Id,
                CustomerName = si.Sale.Customer?.FullName ?? "غير محدد",
                ProductName = si.Product?.Name ?? "منتج محذوف",
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                TotalPrice = si.UnitPrice * si.Quantity,
                SaleDate = si.Sale.SaleDate
            }).ToList();

            foreach (var product in products)
            {
                var productSales = allSales.Where(si => si.ProductId.HasValue && si.ProductId.Value == product.Id).ToList();
                var lastSale = productSales.OrderByDescending(si => si.Sale.SaleDate).FirstOrDefault();

                viewModel.Products.Add(new ProductStatisticsViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Quantity = product.Quantity,
                    PurchasePrice = product.PurchasePrice,
                    SalePrice = product.SalePrice,
                    Barcode = product.Barcode,
                    SalesCount = productSales.Count,
                    TotalSalesValue = productSales.Sum(si => si.UnitPrice * si.Quantity),
                    Profit = productSales.Sum(si => (si.UnitPrice - product.PurchasePrice) * si.Quantity),
                    StockStatus = product.Quantity == 0 ? "نفذ" :
                                 product.Quantity < 5 ? "كمية قليلة" : "متوفر",
                    LastSaleDate = lastSale?.Sale.SaleDate ?? DateTime.MinValue
                });
            }

            if (category.Name.Contains("آيفون") || category.Name.Contains("iPhone"))
            {
                viewModel.IphoneProducts = products.Select(p => new IphoneStatisticsViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Barcode = p.Barcode,
                    Quantity = p.Quantity,
                    PurchasePrice = p.PurchasePrice,
                    SalePrice = p.SalePrice,
                    SalesCount = allSales.Where(si => si.ProductId.HasValue && si.ProductId.Value == p.Id).Count(),
                    TotalSalesValue = allSales.Where(si => si.ProductId.HasValue && si.ProductId.Value == p.Id).Sum(si => si.UnitPrice * si.Quantity),
                    Profit = allSales.Where(si => si.ProductId.HasValue && si.ProductId.Value == p.Id).Sum(si => (si.UnitPrice - p.PurchasePrice) * si.Quantity),
                    LastSaleDate = allSales.Where(si => si.ProductId.HasValue && si.ProductId.Value == p.Id).OrderByDescending(si => si.Sale.SaleDate).FirstOrDefault()?.Sale.SaleDate ?? DateTime.MinValue,
                    StockStatus = p.Quantity == 0 ? "نفذ" :
                                 p.Quantity < 5 ? "كمية قليلة" : "متوفر"
                }).ToList();
            }

            // Pagination info
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Query = q ?? string.Empty;

            return View("Index", viewModel);
        }

        // GET: Products/IPhones (Legacy - Redirect to ByCategory)
     
        public async Task<IActionResult> CategoryStatistics(int categoryId)
        {
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return NotFound();
            }

            var viewModel = new CategoryStatisticsViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                CategoryDescription = category.Description
            };

            // جلب المنتجات في الفئة
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            
            // حساب الإحصائيات الأساسية
            viewModel.TotalProducts = products.Count;
            viewModel.TotalQuantity = products.Sum(p => p.Quantity);
            viewModel.TotalPurchaseValue = products.Sum(p => p.PurchasePrice * p.Quantity);
            viewModel.TotalSaleValue = products.Sum(p => p.SalePrice * p.Quantity);
            viewModel.PotentialProfit = viewModel.TotalSaleValue - viewModel.TotalPurchaseValue;
            viewModel.ProfitMargin = viewModel.TotalPurchaseValue > 0 ? 
                (viewModel.PotentialProfit / viewModel.TotalPurchaseValue) * 100 : 0;

            // إحصائيات المخزون
            var inventorySettings = await _systemSettingsService.GetInventorySettingsAsync();
            viewModel.LowStockProducts = products.Count(p => p.Quantity > 0 && p.Quantity <= inventorySettings.LowStockThreshold);
            viewModel.OutOfStockProducts = products.Count(p => p.Quantity == 0);
            viewModel.HighValueProducts = products.Count(p => p.SalePrice > 1000);

            // جلب المبيعات للمنتجات في هذه الفئة
            var productIds = products.Select(p => p.Id).ToList();
            var allSales = await _context.SaleItems
                .Include(si => si.Sale)
                .ThenInclude(s => s.Customer)
                .Include(si => si.Product)
                .Where(si => si.ProductId.HasValue && productIds.Contains(si.ProductId.Value))
                .ToListAsync();

            var recentSales = allSales
                .OrderByDescending(si => si.Sale.SaleDate)
                .Take(20) // آخر 20 مبيعة
                .ToList();

            viewModel.TotalSalesCount = allSales.Select(si => si.SaleId).Distinct().Count();
            viewModel.TotalSalesValue = allSales.Sum(si => si.UnitPrice * si.Quantity);
            viewModel.TotalUnitsSold = allSales.Sum(si => si.Quantity);
            viewModel.TotalProfit = allSales.Sum(si => (si.UnitPrice - si.Product.PurchasePrice) * si.Quantity);

            // تحويل المبيعات إلى ViewModel
            viewModel.RecentSales = recentSales.Select(si => new SaleStatisticsViewModel
            {
                SaleId = si.Sale.Id,
                CustomerName = si.Sale.Customer?.FullName ?? "غير محدد",
                ProductName = si.Product?.Name ?? "منتج محذوف",
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                TotalPrice = si.UnitPrice * si.Quantity,
                SaleDate = si.Sale.SaleDate,

            }).ToList();

            // تحويل المنتجات إلى ViewModel مع إحصائيات المبيعات
            foreach (var product in products)
            {
                var productSales = allSales.Where(si => si.ProductId.HasValue && si.ProductId.Value == product.Id).ToList();
                var lastSale = productSales.OrderByDescending(si => si.Sale.SaleDate).FirstOrDefault();

                viewModel.Products.Add(new ProductStatisticsViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Quantity = product.Quantity,
                    PurchasePrice = product.PurchasePrice,
                    SalePrice = product.SalePrice,
                    Barcode = product.Barcode,

                    SalesCount = productSales.Count,
                    TotalSalesValue = productSales.Sum(si => si.UnitPrice * si.Quantity),
                    Profit = productSales.Sum(si => (si.UnitPrice - product.PurchasePrice) * si.Quantity),
                    StockStatus = product.Quantity == 0 ? "نفذ" : 
                                 product.Quantity <= inventorySettings.LowStockThreshold ? "كمية قليلة" : "متوفر",
                    LastSaleDate = lastSale?.Sale.SaleDate ?? DateTime.MinValue
                });
            }

           
            return View(viewModel);
        }

        public async Task<IActionResult> Reports()
        {
            var viewModel = new CategoryStatisticsViewModel
            {
                CategoryId = 0,
                CategoryName = "تقارير المنتجات الشاملة",
                CategoryDescription = "إحصائيات شاملة لجميع المنتجات والفئات"
            };

            // جلب جميع المنتجات
            var allProducts = await _productService.GetAllProductsAsync();
            
            // حساب الإحصائيات الأساسية
            viewModel.TotalProducts = allProducts.Count;
            viewModel.TotalQuantity = allProducts.Sum(p => p.Quantity);
            viewModel.TotalPurchaseValue = allProducts.Sum(p => p.PurchasePrice * p.Quantity);
            viewModel.TotalSaleValue = allProducts.Sum(p => p.SalePrice * p.Quantity);
            viewModel.PotentialProfit = viewModel.TotalSaleValue - viewModel.TotalPurchaseValue;
            viewModel.ProfitMargin = viewModel.TotalPurchaseValue > 0 ? 
                (viewModel.PotentialProfit / viewModel.TotalPurchaseValue) * 100 : 0;

            // إحصائيات المخزون
            var inventorySettings = await _systemSettingsService.GetInventorySettingsAsync();
            viewModel.LowStockProducts = allProducts.Count(p => p.Quantity > 0 && p.Quantity <= inventorySettings.LowStockThreshold);
            viewModel.OutOfStockProducts = allProducts.Count(p => p.Quantity == 0);
            viewModel.HighValueProducts = allProducts.Count(p => p.SalePrice > 1000);

            // جلب جميع المبيعات
            var allSales = await _context.SaleItems
                .Include(si => si.Sale)
                .ThenInclude(s => s.Customer)
                .Include(si => si.Product)
                .ToListAsync();

            var recentSales = allSales
                .OrderByDescending(si => si.Sale.SaleDate)
                .Take(50) // آخر 50 مبيعة
                .ToList();

            viewModel.TotalSalesCount = allSales.Select(si => si.SaleId).Distinct().Count();
            viewModel.TotalSalesValue = allSales.Sum(si => si.UnitPrice * si.Quantity);
            viewModel.TotalUnitsSold = allSales.Sum(si => si.Quantity);
            viewModel.TotalProfit = allSales.Sum(si => (si.UnitPrice - si.Product.PurchasePrice) * si.Quantity);

            // تحويل المبيعات إلى ViewModel
            viewModel.RecentSales = recentSales.Select(si => new SaleStatisticsViewModel
            {
                SaleId = si.Sale.Id,
                CustomerName = si.Sale.Customer?.FullName ?? "غير محدد",
                ProductName = si.Product?.Name ?? "منتج محذوف",
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                TotalPrice = si.UnitPrice * si.Quantity,
                SaleDate = si.Sale.SaleDate,
                
            }).ToList();

            // تحويل المنتجات إلى ViewModel مع إحصائيات المبيعات
            foreach (var product in allProducts)
            {
                var productSales = allSales.Where(si => si.ProductId.HasValue && si.ProductId.Value == product.Id).ToList();
                var lastSale = productSales.OrderByDescending(si => si.Sale.SaleDate).FirstOrDefault();

                viewModel.Products.Add(new ProductStatisticsViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Quantity = product.Quantity,
                    PurchasePrice = product.PurchasePrice,
                    SalePrice = product.SalePrice,
                    Barcode = product.Barcode,

                    SalesCount = productSales.Count,
                    TotalSalesValue = productSales.Sum(si => si.UnitPrice * si.Quantity),
                    Profit = productSales.Sum(si => (si.UnitPrice - product.PurchasePrice) * si.Quantity),
                    StockStatus = product.Quantity == 0 ? "نفذ" : 
                                 product.Quantity <= inventorySettings.LowStockThreshold ? "كمية قليلة" : "متوفر",
                    LastSaleDate = lastSale?.Sale.SaleDate ?? DateTime.MinValue
                });
            }

          

            return View("CategoryStatistics", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                // Get all products
                var products = await _productService.GetAllProductsAsync();

                // Get inventory settings for low stock threshold
                var inventorySettings = await _systemSettingsService.GetInventorySettingsAsync();
                var lowStockThreshold = inventorySettings.LowStockThreshold;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("المنتجات");

                    // Set RTL for Arabic text
                    worksheet.View.RightToLeft = true;

                    // Add headers in Arabic
                    worksheet.Cells[1, 1].Value = "اسم المنتج";
                    worksheet.Cells[1, 2].Value = "الفئة";
                    worksheet.Cells[1, 3].Value = "الكمية";
                    worksheet.Cells[1, 4].Value = "سعر الشراء ";
                    worksheet.Cells[1, 5].Value = "سعر البيع";
                    worksheet.Cells[1, 6].Value = "الباركود";
                    worksheet.Cells[1, 7].Value = "الوصف";
                    worksheet.Cells[1, 8].Value = "تاريخ الإنشاء";
                    worksheet.Cells[1, 9].Value = "تاريخ التصدير";

                    // Style the header row
                    using (var range = worksheet.Cells[1, 1, 1, 9])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Color.SetColor(Color.White);
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(54, 96, 146));
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    // Add data rows
                    int row = 2;
                    foreach (var product in products)
                    {
                        worksheet.Cells[row, 1].Value = product.Name;
                        worksheet.Cells[row, 2].Value = product.Category?.Name ?? "-";
                        worksheet.Cells[row, 3].Value = product.Quantity;
                        worksheet.Cells[row, 4].Value = product.PurchasePrice;
                        worksheet.Cells[row, 5].Value = product.SalePrice;
                        worksheet.Cells[row, 6].Value = product.Barcode ?? "-";
                        worksheet.Cells[row, 7].Value = product.Description ?? "";
                        worksheet.Cells[row, 8].Value = ""; // تاريخ الإنشاء غير متوفر حالياً
                        worksheet.Cells[row, 9].Value = DateTime.Now.ToString("yyyy-MM-dd");

                        // Format currency columns
                        worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.000 \"د.ك\"";
                        worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.000 \"د.ك\"";

                        // Highlight low stock items
                        if (product.Quantity <= lowStockThreshold)
                        {
                            worksheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                        }

                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();

                    // Set specific column widths
                    worksheet.Column(1).Width = 25; // Product name
                    worksheet.Column(2).Width = 15; // Category
                    worksheet.Column(3).Width = 10; // Quantity
                    worksheet.Column(4).Width = 15; // Purchase price
                    worksheet.Column(5).Width = 15; // Sale price
                    worksheet.Column(6).Width = 20; // Barcode
                    worksheet.Column(7).Width = 30; // Description
                    worksheet.Column(8).Width = 15; // Created date
                    worksheet.Column(9).Width = 15; // Export date

                    // Add borders to all used cells
                    var usedRange = worksheet.Cells[1, 1, row - 1, 9];
                    usedRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    usedRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    usedRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    usedRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    // Set workbook properties
                    package.Workbook.Properties.Title = "قائمة المنتجات";
                    package.Workbook.Properties.Subject = "تقرير المنتجات";
                    package.Workbook.Properties.Author = "نظام إدارة المنتجات";
                    package.Workbook.Properties.Created = DateTime.Now;

                    // Generate filename with current date
                    var fileName = $"المنتجات_{DateTime.Now:yyyy-MM-dd}.xlsx";

                    // Return file
                    var fileBytes = package.GetAsByteArray();
                    return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Excel Export Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "حدث خطأ أثناء تصدير البيانات. يرجى المحاولة مرة أخرى." });
                }

                TempData["ErrorMessage"] = "حدث خطأ أثناء تصدير البيانات. يرجى المحاولة مرة أخرى.";
                return RedirectToAction(nameof(Index));
            }
        }



    }
} 