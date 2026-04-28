using DocumentFormat.OpenXml.Spreadsheet;
using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.Services.PaymentOptionServices;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Services.SalesServices
{
    public class SalesService : ISalesService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _user;
        private readonly IPaymentOptionService _paymentOptionService;

        public SalesService(AppDbContext context, IUserService user, IPaymentOptionService paymentOptionService)
        {
            _context = context;
            _user = user;
            _paymentOptionService = paymentOptionService;
        }

        public async Task<List<Sale>> GetAllSalesAsync()
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return new List<Sale>();

            return await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.TenantId == tenantId)
                .OrderByDescending(s => s.SaleDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(List<Sale> Sales, int TotalCount, int TotalPages)> GetSalesPaginatedAsync(
            int page,
            int pageSize,
            string? searchTerm,
            ManageMentSystem.Models.SalePaymentType? paymentType,
            DateTime? fromDate,
            DateTime? toDate
            )
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return (new List<Sale>(), 0, 0);

            var query = _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.TenantId == tenantId)
                .AsNoTracking();

            // تطبيق البحث
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(s =>
                    s.Customer.FullName.ToLower().Contains(searchTerm) ||
                    s.Id.ToString().Contains(searchTerm)
                );
            }

            // تطبيق فلتر التاريخ (Range)
            if (fromDate.HasValue)
            {
                query = query.Where(s => s.SaleDate.Date >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                query = query.Where(s => s.SaleDate.Date <= toDate.Value.Date);
            }

            if (paymentType.HasValue)
            {
                query = query.Where(s => s.PaymentType == paymentType.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var sales = await query
                .OrderByDescending(s => s.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (sales, totalCount, totalPages);
        }

        public async Task<Sale> GetSaleByIdAsync(int id)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return null;

            return await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Where(s => s.TenantId == tenantId)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Sale> AddSaleAsync(CreateSaleViewModel model, string currentUserId)
        {
            try
            {
                var tenantId = await _user.GetCurrentTenantIdAsync();
                var userId = await _user.GetCurrentUserIdAsync();

                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                    throw new InvalidOperationException("المستخدم غير مسجل دخول");

                // ===============================
                // جلب أو إنشاء العميل الافتراضي
                // ===============================
                async Task<Customer> GetOrCreateDefaultCustomerAsync()
                {
                    var defaultCustomer = await _context.Customers
                        .FirstOrDefaultAsync(c =>
                            c.TenantId == tenantId &&
                            c.PhoneNumber == "01111111111");

                    if (defaultCustomer != null)
                        return defaultCustomer;

                    var newCustomer = new Customer
                    {
                        FullName = "بدون عميل",
                        PhoneNumber = "01111111111",
                        Address = "-",
                        TenantId = tenantId,
                    };

                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync();

                    return newCustomer;
                }

                // ===============================
                // التحقق من المخزون
                // ===============================
                if (model.SaleItems != null && model.SaleItems.Any())
                {
                    foreach (var item in model.SaleItems)
                    {
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p =>
                                p.TenantId == tenantId &&
                                p.Id == (item.ProductId ?? 0));

                        if (product == null)
                            throw new InvalidOperationException($"المنتج غير موجود أو ليس لديك صلاحية للوصول إليه");

                        if (product.Quantity < item.Quantity)
                            throw new InvalidOperationException($"الكمية المطلوبة غير متوفرة للمنتج '{product.Name}'");
                    }
                }

                // ===============================
                // تحديد العميل
                // ===============================
                Customer customer;

                if (model.CustomerId.HasValue && model.CustomerId > 0)
                {
                    customer = await _context.Customers
                        .FirstOrDefaultAsync(c =>
                            c.Id == model.CustomerId &&
                            c.TenantId == tenantId)
                        ?? throw new InvalidOperationException("العميل غير موجود");
                }
                else
                {
                    if (model.PaymentType == SalePaymentType.Cash)
                    {
                        customer = await GetOrCreateDefaultCustomerAsync();
                        model.CustomerId = customer.Id;
                    }
                    else
                    {
                        throw new InvalidOperationException("يجب اختيار عميل للبيع الآجل أو الجزئي");
                    }
                }

                // ===============================
                // حساب المبالغ
                // ===============================
                decimal paidAmount = model.PaymentType switch
                {
                    SalePaymentType.Cash => model.TotalAmount,
                    SalePaymentType.Partial => Math.Min(model.PaidAmount, model.TotalAmount),
                    SalePaymentType.Credit => 0m,
                    _ => model.PaidAmount
                };

                var subtotal = model.SaleItems?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
                var discountAmount = subtotal - model.TotalAmount;
                var discountPercentage = subtotal > 0 ? (discountAmount / subtotal) * 100 : 0;

                // ===============================
                // إنشاء البيع
                // ===============================
                var sale = new Sale
                {
                    CustomerId = customer.Id,
                    SaleDate = model.SaleDate,
                    TotalAmount = model.TotalAmount,
                    PaidAmount = paidAmount,
                    DiscountAmount = discountAmount,
                    DiscountPercentage = discountPercentage,
                    PaymentType = model.PaymentType,
                    TenantId = tenantId,
                };

                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();

                // ===============================
                // SaleItems + تحديث المخزون
                // ===============================
                foreach (var item in model.SaleItems)
                {
                    _context.SaleItems.Add(new SaleItem
                    {
                        SaleId = sale.Id,
                        ProductId = item.ProductId.Value,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TenantId = tenantId
                    });

                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Quantity -= item.Quantity;
                        _context.Products.Update(product);
                    }
                }

                await _context.SaveChangesAsync();

                // ===============================
                // حساب المحل (StoreAccount)
                // ===============================
                if (paidAmount > 0)
                {
                    var paymentMethodId = model.PaymentMethodId
                        ?? await _paymentOptionService.GetDefaultIdAsync();

                    var paymentMethod = await _context.PaymentMethodOptions.FindAsync(paymentMethodId);

                    _context.StoreAccounts.Add(new StoreAccount
                    {
                        TransactionName = $"بيع - {customer.FullName}",
                        TransactionType = TransactionType.Income,
                        Amount = paidAmount,
                        TransactionDate = sale.SaleDate,
                        Description = $"بيع منتجات للعميل {customer.FullName} - إجمالي: {subtotal:C} - خصم: {discountAmount:C} - مدفوع: {paidAmount:C} - متبقي: {(sale.TotalAmount - paidAmount):C}",
                        Category = "المبيعات",
                        PaymentMethodId = paymentMethodId,
                        PaymentMethod = paymentMethod,
                        ReferenceNumber = $"SALE-{sale.Id}",
                        SaleId = sale.Id,
                        TenantId = tenantId,
                    });
                }

                // ===============================
                // دفعة العميل (CustomerPayment + PaymentAllocation)
                // ===============================
                if (paidAmount > 0 && customer != null)
                {
                    var defaultPaymentMethodId = model.PaymentMethodId ?? await _paymentOptionService.GetDefaultIdAsync();

                    var customerPayment = new CustomerPayment
                    {
                        CustomerId = customer.Id,  // حتى لو "بدون عميل"
                        Amount = paidAmount,
                        PaymentDate = model.SaleDate,
                        PaymentMethodId = defaultPaymentMethodId,
                        Notes = $"دفعة على فاتورة رقم {sale.Id}",
                        TenantId = tenantId,
                    };
                    _context.CustomerPayments.Add(customerPayment);
                    await _context.SaveChangesAsync();

                    var allocation = new PaymentAllocation
                    {
                        CustomerPaymentId = customerPayment.Id,
                        SaleId = sale.Id,
                        Amount = paidAmount,
                        TenantId = tenantId
                    };
                    _context.PaymentAllocations.Add(allocation);
                }

                await _context.SaveChangesAsync();
                return sale;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Sale> UpdateSaleAsync(int id, CreateSaleViewModel model)
        {
            try
            {
                var tenantId = await _user.GetCurrentTenantIdAsync();
                var userId = await _user.GetCurrentUserIdAsync();
                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                    throw new InvalidOperationException("المستخدم غير مسجل دخول");

                // التحقق من أن البيع تابع للمستخدم
                var sale = await _context.Sales
                    .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                    .Where(s => s.TenantId == tenantId)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sale == null)
                    throw new InvalidOperationException("عملية البيع غير موجودة أو ليس لديك صلاحية للوصول إليها");

                // التحقق من أن العميل الجديد تابع للمستخدم (اختياري)
                Customer? customer = null;
                if (model.CustomerId.HasValue && model.CustomerId > 0)
                {
                    customer = await _context.Customers
                        .Where(c => c.TenantId == tenantId)
                        .FirstOrDefaultAsync(c => c.Id == model.CustomerId);

                    if (customer == null)
                    {
                        throw new InvalidOperationException("العميل غير موجود أو ليس لديك صلاحية للوصول إليه");
                    }
                }

                var oldTotalAmount = sale.TotalAmount;
                var oldSaleItems = sale.SaleItems.ToList();
                var customerName = customer?.FullName ?? "عميل نقدي";

                // إذا لم تُرسل عناصر بيع
                if (model.SaleItems == null || !model.SaleItems.Any())
                {
                    sale.CustomerId = model.CustomerId.HasValue && model.CustomerId > 0 ? model.CustomerId : null;
                    sale.SaleDate = model.SaleDate;

                    var storeTxn = await _context.StoreAccounts
                        .FirstOrDefaultAsync(sa => sa.SaleId == sale.Id);

                    if (storeTxn != null)
                    {
                        storeTxn.TransactionName = $"بيع - {customerName}";
                        storeTxn.TransactionDate = model.SaleDate;
                        var prevTotalBeforeDiscount = sale.TotalAmount + sale.DiscountAmount;
                        storeTxn.Description = $"بيع منتجات للعميل {customerName} - إجمالي: {prevTotalBeforeDiscount:C} - خصم: {sale.DiscountAmount:C} - مدفوع: {sale.PaidAmount:C} - متبقي: {(sale.TotalAmount - sale.PaidAmount):C}";
                        _context.StoreAccounts.Update(storeTxn);
                    }

                    _context.Sales.Update(sale);
                    await _context.SaveChangesAsync();
                    return sale;
                }

                // حساب المبالغ الجديدة
                var subtotal = model.SaleItems?.Sum(item => item.Quantity * item.UnitPrice) ?? 0;
                var totalBeforeDiscount = subtotal;
                var totalAfterDiscount = model.TotalAmount;
                var discountAmount = totalBeforeDiscount - totalAfterDiscount;
                var discountPercentage = totalBeforeDiscount > 0 ? (discountAmount / totalBeforeDiscount) * 100 : 0;

                if (totalAfterDiscount < 0 || totalAfterDiscount > totalBeforeDiscount)
                {
                    throw new InvalidOperationException("المبلغ بعد الخصم غير صالح.");
                }

                // تحديث بيانات البيع
                sale.CustomerId = model.CustomerId.HasValue && model.CustomerId > 0 ? model.CustomerId : null;
                sale.SaleDate = model.SaleDate;
                sale.TotalAmount = totalAfterDiscount;
                sale.DiscountPercentage = discountPercentage;
                sale.DiscountAmount = discountAmount;

                // التعامل مع المبلغ المدفوع
                if (sale.PaymentType == SalePaymentType.Cash)
                {
                    sale.PaidAmount = totalAfterDiscount;
                }
                else if (sale.PaymentType == SalePaymentType.Credit)
                {
                    sale.PaidAmount = 0;
                }

                if (sale.PaidAmount > sale.TotalAmount)
                {
                    throw new InvalidOperationException("لا يمكن أن يكون المبلغ المدفوع أكبر من إجمالي الفاتورة.");
                }

                // تحديث المنتجات - مع التحقق من UserId
                if (model.SaleItems != null && model.SaleItems.Any())
                {
                    // إرجاع الكميات القديمة
                    foreach (var oldItem in oldSaleItems)
                    {
                        var product = await _context.Products
                            .Where(p => p.TenantId == tenantId)
                            .FirstOrDefaultAsync(p => p.Id == oldItem.ProductId);

                        if (product != null)
                        {
                            product.Quantity += oldItem.Quantity;
                            _context.Products.Update(product);
                        }
                    }

                    _context.SaleItems.RemoveRange(oldSaleItems);

                    // إضافة العناصر الجديدة
                    var newSaleItems = new List<SaleItem>();
                    foreach (var item in model.SaleItems)
                    {
                        var product = await _context.Products
                            .Where(p => p.TenantId == tenantId)
                            .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                        if (product == null)
                        {
                            throw new InvalidOperationException($"المنتج غير موجود أو ليس لديك صلاحية للوصول إليه");
                        }

                        if (product.Quantity < item.Quantity)
                        {
                            throw new InvalidOperationException($"الكمية المطلوبة غير متوفرة للمنتج '{product.Name}'. المتوفر: {product.Quantity}");
                        }

                        product.Quantity -= item.Quantity;
                        _context.Products.Update(product);

                        var saleItem = new SaleItem
                        {
                            SaleId = sale.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        };
                        newSaleItems.Add(saleItem);
                    }

                    _context.SaleItems.AddRange(newSaleItems);
                }
                else
                {
                    // إرجاع الكميات وحذف العناصر
                    foreach (var oldItem in oldSaleItems)
                    {
                        var product = await _context.Products
                            .Where(p => p.TenantId == tenantId)
                            .FirstOrDefaultAsync(p => p.Id == oldItem.ProductId);

                        if (product != null)
                        {
                            product.Quantity += oldItem.Quantity;
                            _context.Products.Update(product);
                        }
                    }

                    _context.SaleItems.RemoveRange(oldSaleItems);

                    totalAfterDiscount = 0;
                    discountAmount = 0;
                    sale.TotalAmount = 0;
                    sale.DiscountAmount = 0;
                    sale.DiscountPercentage = 0;

                    if (sale.PaymentType != SalePaymentType.Cash)
                    {
                        sale.PaidAmount = 0;
                    }
                }

                await HandleSaleUpdateByPaymentType(sale, model, oldTotalAmount, totalAfterDiscount, totalBeforeDiscount, discountAmount);

                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();
                return sale;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"فشل في تحديث البيع: {ex.Message}");
            }
        }

        public async Task<bool> DeleteSaleAsync(int id)
        {
            try
            {
                var tenantId = await _user.GetCurrentTenantIdAsync();
                if (string.IsNullOrEmpty(tenantId)) return false;

                // التحقق من أن البيع تابع للمستخدم
                var sale = await _context.Sales
                    .Include(s => s.SaleItems)
                    .Where(s => s.TenantId == tenantId)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sale == null)
                {
                    return false;
                }


                // استرجاع التخصيصات المرتبطة بالفاتورة مع الدفعات
                var allocations = await _context.PaymentAllocations
                    .Include(pa => pa.CustomerPayment)
                    .Where(pa => pa.SaleId == sale.Id)
                    .ToListAsync();

                foreach (var allocation in allocations)
                {
                    var payment = allocation.CustomerPayment;

                    // خصم المبلغ المخصص من إجمالي الدفعة
                    payment.Amount -= allocation.Amount;

                    // البحث عن قيد الخزنة المرتبط بالدفعة (عادة من AddPaymentAsync)
                    var paymentStoreTx = await _context.StoreAccounts
                        .FirstOrDefaultAsync(sa => sa.ReferenceNumber == $"CPAY-{payment.Id}");

                    if (payment.Amount <= 0)
                    {
                        // إذا أصبح مبلغ الدفعة صفر أو أقل، نحذف الدفعة وقيد الخزنة المرتبط بها
                        if (paymentStoreTx != null)
                        {
                            _context.StoreAccounts.Remove(paymentStoreTx);
                        }

                        _context.CustomerPayments.Remove(payment);
                    }
                    else
                    {
                        // تحديث مبلغ الدفعة
                        _context.CustomerPayments.Update(payment);

                        // تحديث قيد الخزنة المرتبط إذا وجد
                        if (paymentStoreTx != null)
                        {
                            paymentStoreTx.Amount = payment.Amount;
                            _context.StoreAccounts.Update(paymentStoreTx);
                        }
                    }

                    // حذف التخصيص
                    _context.PaymentAllocations.Remove(allocation);
                }

                // حذف العملية من حساب المحل
                var storeTransaction = await _context.StoreAccounts
                    .FirstOrDefaultAsync(sa => sa.SaleId == sale.Id);

                if (storeTransaction != null)
                {
                    _context.StoreAccounts.Remove(storeTransaction);
                }

                // استرجاع المنتجات
                foreach (var item in sale.SaleItems)
                {
                    var product = await _context.Products
                        .Where(p => p.TenantId == tenantId)
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product != null)
                    {
                        product.Quantity += item.Quantity;
                        _context.Products.Update(product);
                    }
                }

                _context.SaleItems.RemoveRange(sale.SaleItems);
                _context.Sales.Remove(sale);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<Sale>> GetSalesWithDetailsAsync()
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return new List<Sale>();

            return await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .OrderByDescending(s => s.SaleDate)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return new List<Customer>();

            return await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return new List<Product>();

            return await _context.Products
                .Where(p => p.TenantId == tenantId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<PaymentMethodOption>> GetPaymentMethodsAsync()
        {
            return await _paymentOptionService.GetActiveAsync();
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            try
            {
                var tenantId = await _user.GetCurrentTenantIdAsync();
                var userId = await _user.GetCurrentUserIdAsync();
                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId)) return null;

                customer.TenantId = tenantId;
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return customer;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int> GetTotalAccountProduct(string id, DateTime? startDate = null, DateTime? endDate = null)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return 0;
            var query = _context.Sales.Where(s => s.TenantId == tenantId);

            if (startDate.HasValue)
            {
                query = query.Where(s => s.SaleDate.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(s => s.SaleDate.Date <= endDate.Value.Date);
            }

            return await query
                .SelectMany(s => s.SaleItems)
                .SumAsync(si => si.Quantity);
        }

        public async Task<int> GetTotalProduct(string id, DateTime? startDate = null, DateTime? endDate = null)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return 0;
            var query = _context.Sales.Where(s => s.TenantId == tenantId).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(s => s.SaleDate.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(s => s.SaleDate.Date <= endDate.Value.Date);
            }

            return await query
                .SelectMany(s => s.SaleItems)
                .SumAsync(si => si.Quantity);
        }

        private async Task HandleSaleUpdateByPaymentType(Sale sale, CreateSaleViewModel model, decimal oldTotalAmount, decimal newTotalAmount, decimal totalBeforeDiscount, decimal discountAmount)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            var userId = await _user.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId)) return;

            Customer? customer = null;
            if (model.CustomerId.HasValue && model.CustomerId > 0)
            {
                customer = await _context.Customers
                    .Where(c => c.TenantId == tenantId)
                    .FirstOrDefaultAsync(c => c.Id == model.CustomerId);
            }

            var defaultPaymentMethodId = await _paymentOptionService.GetDefaultIdAsync();

            switch (sale.PaymentType)
            {
                case SalePaymentType.Cash:
                    await HandleCashInvoiceUpdate(sale, model, oldTotalAmount, newTotalAmount, totalBeforeDiscount, discountAmount, customer, defaultPaymentMethodId);
                    break;

                case SalePaymentType.Partial:
                    await HandlePartialInvoiceUpdate(sale, model, oldTotalAmount, newTotalAmount, totalBeforeDiscount, discountAmount, customer, defaultPaymentMethodId);
                    break;

                case SalePaymentType.Credit:
                    await HandleCreditInvoiceUpdate(sale, model, oldTotalAmount, newTotalAmount, totalBeforeDiscount, discountAmount, customer, defaultPaymentMethodId);
                    break;
            }
        }

        private async Task HandleCashInvoiceUpdate(Sale sale, CreateSaleViewModel model, decimal oldTotalAmount, decimal newTotalAmount, decimal totalBeforeDiscount, decimal discountAmount, Customer? customer, int? defaultPaymentMethodId)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            var userId = await _user.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId)) return;

            var storeTransaction = await _context.StoreAccounts
                .FirstOrDefaultAsync(sa => sa.SaleId == sale.Id);

            if (storeTransaction != null)
            {
                if (newTotalAmount == 0)
                {
                    _context.StoreAccounts.Remove(storeTransaction);
                }
                else
                {
                    storeTransaction.TransactionName = $"بيع - {customer?.FullName ?? "عميل"}";
                    storeTransaction.Amount = newTotalAmount;
                    storeTransaction.TransactionDate = model.SaleDate;
                    storeTransaction.Description = $"بيع منتجات للعميل {customer?.FullName} - إجمالي: {totalBeforeDiscount:C} - خصم: {discountAmount:C} - مدفوع: {newTotalAmount:C} - متبقي: 0.00";
                    _context.StoreAccounts.Update(storeTransaction);
                }
            }
            else if (newTotalAmount > 0)
            {
                var newStoreTransaction = new StoreAccount
                {
                    TransactionName = $"بيع - {customer?.FullName ?? "عميل"}",
                    TransactionType = TransactionType.Income,
                    Amount = newTotalAmount,
                    TransactionDate = model.SaleDate,
                    Description = $"بيع منتجات للعميل {customer?.FullName} - إجمالي: {totalBeforeDiscount:C} - خصم: {discountAmount:C} - مدفوع: {newTotalAmount:C} - متبقي: 0.00",
                    Category = "المبيعات",
                    TenantId = tenantId,
                    PaymentMethodId = defaultPaymentMethodId,
                    ReferenceNumber = $"SALE-{sale.Id}",
                    SaleId = sale.Id
                };
                _context.StoreAccounts.Add(newStoreTransaction);
            }

            var allocationsForSale = await _context.PaymentAllocations
                .Include(pa => pa.CustomerPayment)
                .Where(pa => pa.SaleId == sale.Id)
                .ToListAsync();

            if (newTotalAmount == 0)
            {
                foreach (var allocation in allocationsForSale)
                {
                    var payment = allocation.CustomerPayment;
                    payment.Amount -= allocation.Amount;
                    _context.PaymentAllocations.Remove(allocation);

                    var hasOtherAllocations = await _context.PaymentAllocations.AnyAsync(pa => pa.CustomerPaymentId == payment.Id);
                    if (!hasOtherAllocations && payment.Amount <= 0)
                    {
                        _context.CustomerPayments.Remove(payment);
                    }
                    else
                    {
                        _context.CustomerPayments.Update(payment);
                    }
                }
            }
            else
            {
                if (allocationsForSale.Any())
                {
                    var allocation = allocationsForSale.First();
                    var payment = allocation.CustomerPayment;

                    var previousAllocationAmount = allocation.Amount;
                    allocation.Amount = newTotalAmount;

                    payment.Amount = payment.Amount - previousAllocationAmount + newTotalAmount;
                    payment.CustomerId = sale.CustomerId ?? 0;
                    payment.PaymentDate = model.SaleDate;
                    payment.PaymentMethodId = payment.PaymentMethodId ?? defaultPaymentMethodId;
                    payment.Notes = $"دفعة على فاتورة رقم {sale.Id}";

                    _context.PaymentAllocations.Update(allocation);
                    _context.CustomerPayments.Update(payment);
                }
                else
                {
                    var newPayment = new CustomerPayment
                    {
                        CustomerId = sale.CustomerId ?? 0,
                        Amount = newTotalAmount,
                        PaymentDate = model.SaleDate,
                        PaymentMethodId = defaultPaymentMethodId,
                        Notes = $"دفعة على فاتورة رقم {sale.Id}"
                    };
                    _context.CustomerPayments.Add(newPayment);
                    await _context.SaveChangesAsync();

                    var newAllocation = new PaymentAllocation
                    {
                        CustomerPaymentId = newPayment.Id,
                        SaleId = sale.Id,
                        Amount = newTotalAmount
                    };
                    _context.PaymentAllocations.Add(newAllocation);
                }
            }
        }

        private async Task HandleCreditInvoiceUpdate(Sale sale, CreateSaleViewModel model, decimal oldTotalAmount, decimal newTotalAmount, decimal totalBeforeDiscount, decimal discountAmount, Customer? customer, int? defaultPaymentMethodId)
        {
            // No code - implement if needed
        }

        private async Task HandlePartialInvoiceUpdate(Sale sale, CreateSaleViewModel model, decimal oldTotalAmount, decimal newTotalAmount, decimal totalBeforeDiscount, decimal discountAmount, Customer? customer, int? defaultPaymentMethodId)
        {
            // No code - implement if needed
        }

        public async Task<(List<Sale> Sales, int TotalCount, int TotalPages, decimal TotalUnpaidAmount)>
     GetUnpaidSalesAsync(
         DateTime? fromDate = null,
         DateTime? toDate = null,
         int page = 1,
         int pageSize = 20)
        {
            // 1️⃣ جلب TenantId
            var tenantId = await _user.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId))
                return (new List<Sale>(), 0, 0, 0m);

            // 2️⃣ بناء الـ Query الأساسية
            var query = _context.Sales
                .Include(s => s.Customer)
                .Where(s =>
                    s.TenantId == tenantId && // 🔑 أهم سطر
                    (s.PaymentType == SalePaymentType.Partial ||
                     s.PaymentType == SalePaymentType.Credit) &&
                    (s.TotalAmount - s.ReturnedAmount - s.PaidAmount) > 0
                )
                .AsNoTracking()
                .AsQueryable();

            // 3️⃣ فلترة من تاريخ
            if (fromDate.HasValue)
            {
                query = query.Where(s => s.SaleDate >= fromDate.Value);
            }

            // 4️⃣ فلترة إلى تاريخ
            if (toDate.HasValue)
            {
                query = query.Where(s => s.SaleDate <= toDate.Value);
            }

            // 5️⃣ إجمالى عدد النتائج
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 6️⃣ إجمالى المبلغ المتبقى (لكل النتائج مش الصفحة بس)
            var totalUnpaidAmount = await query.SumAsync(s =>
                (s.TotalAmount - s.ReturnedAmount - s.PaidAmount) > 0
                    ? (s.TotalAmount - s.ReturnedAmount - s.PaidAmount)
                    : 0m
            );

            // 7️⃣ بيانات الصفحة الحالية
            var sales = await query
                .OrderByDescending(s => s.SaleDate)
                .ThenBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 8️⃣ الرجوع بالنتيجة
            return (sales, totalCount, totalPages, totalUnpaidAmount);
        }

    }
}
