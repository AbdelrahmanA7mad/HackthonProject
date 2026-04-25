using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.PaymentOptionServices;
using ManageMentSystem.Services.StoreAccountServices;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IStoreAccountService _storeAccountService;
        private readonly IUserService _userService;
        private readonly IPaymentOptionService _paymentOptionService;

        public ProductService(AppDbContext context, IStoreAccountService storeAccountService, IUserService userService, IPaymentOptionService paymentOptionService)
        {
            _context = context;
            _storeAccountService = storeAccountService;
            _userService = userService;
            _paymentOptionService = paymentOptionService;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var rootUserId = await _userService.GetRootUserIdAsync();
            return await _context.Products
                .Where(p => p.TenantId == rootUserId)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .AsNoTracking() // تحسين الأداء - عدم تتبع التغييرات
                .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            var rootUserId = await _userService.GetRootUserIdAsync();
            return await _context.Products
                .Where(p => p.TenantId == rootUserId)
                .Include(p => p.Category)
                .AsNoTracking() // تحسين الأداء - للقراءة فقط
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(bool Success, string Message, Product? Product)> AddProductWithBalanceCheckAsync(CreateProductViewModel model)
        {
            // التأكد من عدم تكرار الباركود لنفس المستخدم
            var rootUserId = await _userService.GetRootUserIdAsync();
            if (!string.IsNullOrWhiteSpace(model.Barcode))
            {
                var existingBarcode = await _context.Products
                    .Where(p => p.TenantId == rootUserId)
                    .FirstOrDefaultAsync(p => p.Barcode == model.Barcode.Trim());
                if (existingBarcode != null)
                {
                    return (false, $"الباركود '{model.Barcode}' مستخدم بالفعل مع منتج آخر: '{existingBarcode.Name}'. يرجى استخدام باركود مختلف.", existingBarcode);
                }
            }

            // حساب التكلفة الإجمالية للمنتجات المضافة
            var totalCost = model.PurchasePrice * model.Quantity;

            // الحصول على طريقة الدفع الافتراضية
            var defaultPaymentMethodId = await _paymentOptionService.GetDefaultIdAsync();

            var currentBalance = await _storeAccountService.GetCashBalanceByPaymentMethodAsync(defaultPaymentMethodId);

            if (currentBalance < totalCost)
            {
                return (false, $"رصيد الخزنة غير كافٍ. الرصيد الحالي: {currentBalance:C}، والتكلفة المطلوبة: {totalCost:C}", null);
            }

            var userId = await _userService.GetCurrentUserIdAsync();
            var product = new Product
            {
                Name = model.Name,
                Quantity = model.Quantity,
                Description = model.Description,
                PurchasePrice = model.PurchasePrice,
                SalePrice = model.SalePrice,
                Barcode = !string.IsNullOrWhiteSpace(model.Barcode) ? model.Barcode.Trim() : null,
                CategoryId = model.CategoryId ?? 1,
                TenantId = rootUserId,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var purchaseTransaction = new StoreAccountViewModel
            {
                TransactionName = $"شراء منتج - {model.Name}",
                TransactionType = TransactionType.Expense,
                Amount = totalCost,
                TransactionDate = DateTime.Now,
                Description = $"شراء {model.Quantity} قطعة من {model.Name} بسعر {model.PurchasePrice:C} للقطعة",
                Category = "مشتريات",
                PaymentMethodId = defaultPaymentMethodId,
                ReferenceNumber = $"PURCHASE-{product.Id}",
                Notes = $"عملية إضافة منتج - {model.Name}"
            };

            await _storeAccountService.CreateTransactionAsync(purchaseTransaction);

            return (true, $"تمت إضافة المنتج وخصم مبلغ {totalCost:C} من رصيد الخزنة", product);
        }

        public async Task<(bool Success, string Message, Product? Product)> UpdateProductAsync(int id, CreateProductViewModel model)
        {
            var currentUserIdForUpdate = await _userService.GetRootUserIdAsync();
            var product = await _context.Products
                .Where(p => p.TenantId == currentUserIdForUpdate)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return (false, $"المنتج غير موجود", null);

            // التحقق من الباركود (باستثناء المنتج الحالي)
            if (!string.IsNullOrWhiteSpace(model.Barcode))
            {
                var existingBarcode = await _context.Products
                    .Where(p => p.TenantId == currentUserIdForUpdate)
                    .FirstOrDefaultAsync(p => p.Barcode == model.Barcode.Trim() && p.Id != id);
                if (existingBarcode != null)
                {
                    return (false, $"الباركود '{model.Barcode}' مستخدم بالفعل مع منتج آخر: '{existingBarcode.Name}'.", existingBarcode);
                }
            }

            // حساب فرق التكلفة بين القديم والجديد
            var oldTotalCost = product.PurchasePrice * product.Quantity;
            var newTotalCost = model.PurchasePrice * model.Quantity;
            var costDifference = newTotalCost - oldTotalCost;

            if (costDifference > 0)
            {
                var defaultPaymentMethodId = await _paymentOptionService.GetDefaultIdAsync();
                var currentBalance = await _storeAccountService.GetCashBalanceByPaymentMethodAsync(defaultPaymentMethodId);

                if (currentBalance < costDifference)
                {
                    return (false, $"الرصيد غير كافٍ لتغطية الزيادة في التكلفة. الرصيد الحالي: {currentBalance:C}، الفرق المطلوب: {costDifference:C}", null);
                }
            }

            product.Name = model.Name;
            product.Quantity = model.Quantity;
            product.Description = model.Description;
            product.PurchasePrice = model.PurchasePrice;
            product.SalePrice = model.SalePrice;
            product.Barcode = !string.IsNullOrWhiteSpace(model.Barcode) ? model.Barcode.Trim() : null;
            product.CategoryId = model.CategoryId ?? 1;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            if (costDifference != 0)
            {
                var transactionType = costDifference > 0 ? TransactionType.Expense : TransactionType.Income;
                var transactionName = costDifference > 0 ? "تحديث منتج - زيادة تكلفة" : "تحديث منتج - استرداد فرق";
                var amount = Math.Abs(costDifference);

                var defaultPaymentMethodId = await _paymentOptionService.GetDefaultIdAsync();

                var updateTransaction = new StoreAccountViewModel
                {
                    TransactionName = $"{transactionName} - {model.Name}",
                    TransactionType = transactionType,
                    Amount = amount,
                    TransactionDate = DateTime.Now,
                    Description = $"تعديل {model.Name} - التكلفة القديمة: {oldTotalCost:C}، التكلفة الجديدة: {newTotalCost:C}، الفرق: {costDifference:C}",
                    Category = "تعديل مخزون",
                    PaymentMethodId = defaultPaymentMethodId,
                    ReferenceNumber = $"UPDATE-{product.Id}",
                    Notes = $"تعديل بيانات منتج - {model.Name}"
                };

                await _storeAccountService.CreateTransactionAsync(updateTransaction);

                var message = costDifference > 0
                    ? $"تم تحديث المنتج وخصم فرق {costDifference:C} من الرصيد"
                    : $"تم تحديث المنتج وإعادة فرق {Math.Abs(costDifference):C} إلى الرصيد";

                return (true, message, product);
            }

            return (true, "تم تحديث بيانات المنتج بنجاح بدون تغيير في التكلفة", product);
        }

        public async Task<(bool Success, string Message)> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return (false, "المنتج غير موجود");

            // التحقق من وجود عمليات بيع مرتبطة بهذا المنتج
            var hasSales = await _context.SaleItems.AnyAsync(si => si.ProductId.HasValue && si.ProductId.Value == id);

            if (hasSales)
            {
                // إذا كان هناك مبيعات، يفضل حذف المنتج منطقياً أو إبقاؤه (هنا تم الإبقاء على كود الحذف مع تنبيه)
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                string operations = "مبيعات";
                return (true, $"تم حذف المنتج بنجاح. لاحظ وجود عمليات مرتبطة سابقة: {operations}");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return (true, "تم حذف المنتج بنجاح");
        }

        public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            var currentUserId = await _userService.GetRootUserIdAsync();
            return await _context.Products
                .Where(p => p.TenantId == currentUserId)
                .Include(p => p.Category)
                .Where(p => p.Quantity <= threshold)
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            var currentUserId = await _userService.GetRootUserIdAsync();
            return await _context.Products
                .Where(p => p.TenantId == currentUserId)
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<bool> IsBarcodeUniqueAsync(string barcode, int? excludeProductId = null)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return true;

            var currentUserId = await _userService.GetRootUserIdAsync();
            var query = _context.Products
                .Where(p => p.TenantId == currentUserId)
                .Where(p => p.Barcode == barcode.Trim());

            if (excludeProductId.HasValue)
                query = query.Where(p => p.Id != excludeProductId.Value);

            return !await query.AnyAsync();
        }

        public async Task<Product?> GetProductByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return null;

            var currentUserId = await _userService.GetRootUserIdAsync();
            return await _context.Products
                .Where(p => p.TenantId == currentUserId)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Barcode == barcode.Trim());
        }
    }
}