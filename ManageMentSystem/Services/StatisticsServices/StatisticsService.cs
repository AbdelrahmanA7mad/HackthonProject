using ManageMentSystem.Data;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using ManageMentSystem.Models;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.Services.SystemSettings;

namespace ManageMentSystem.Services.StatisticsServices
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;
        private readonly ISystemSettingsService _systemSettingsService;
        private readonly IUserService _userService;
        public StatisticsService(AppDbContext context, ISystemSettingsService systemSettingsService, IUserService userService)
        {
            _context = context;
            _systemSettingsService = systemSettingsService;
            _userService = userService;
        }
        public async Task<StatisticsViewModel> GetStatisticsAsync()
        {
            return await GetFilteredStatisticsAsync("all", null, null, null);
        }

        public async Task<StatisticsViewModel> GetFilteredStatisticsAsync(
      string period,
      DateTime? fromDate,
      DateTime? toDate,
      int? customerId)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var currentDate = DateTime.Now;

            var statistics = new StatisticsViewModel
            {
                Period = period,
                FromDate = fromDate,
                ToDate = toDate,
                CustomerId = customerId
            };

            // Get customers for filter dropdown
            statistics.Customers = await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .OrderBy(c => c.FullName)
                .ToListAsync();

            // Set customer name if selected
            if (customerId.HasValue)
            {
                var customer = statistics.Customers.FirstOrDefault(c => c.Id == customerId.Value);
                statistics.CustomerName = customer?.FullName;
            }

            // Calculate date range based on period
            DateTime? periodStart = null;
            DateTime? periodEnd = null;

            switch (period)
            {
                case "today":
                    periodStart = currentDate.Date;
                    periodEnd = currentDate;
                    break;
                case "week":
                    periodStart = currentDate.AddDays(-(int)currentDate.DayOfWeek);
                    periodEnd = currentDate;
                    break;
                case "month":
                    periodStart = new DateTime(currentDate.Year, currentDate.Month, 1);
                    periodEnd = currentDate;
                    break;
                case "year":
                    periodStart = new DateTime(currentDate.Year, 1, 1);
                    periodEnd = currentDate;
                    break;
                case "custom":
                    periodStart = fromDate;
                    periodEnd = toDate;
                    break;
            }

            // Build base sales query (filtered by tenant)
            var salesQuery = _context.Sales
                .Where(s => s.TenantId == tenantId)
                .AsQueryable();

            // Apply date filters
            if (periodStart.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate >= periodStart.Value);

            if (periodEnd.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate <= periodEnd.Value);

            // Apply customer filter
            if (customerId.HasValue)
                salesQuery = salesQuery.Where(s => s.CustomerId == customerId.Value);

            // General statistics (filtered by tenant)
            statistics.TotalSales = await _context.Sales
                .CountAsync(s => s.TenantId == tenantId);

            statistics.TotalRevenue = await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            statistics.TotalCustomers = await _context.Customers
                .CountAsync(c => c.TenantId == tenantId);

            statistics.TotalProducts = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .SumAsync(p => (int?)p.Quantity) ?? 0;

            statistics.TotalAllRevenue = statistics.TotalRevenue;

            statistics.LowStockCount = await _context.Products
                .CountAsync(p => p.TenantId == tenantId && p.Quantity <= 5);

            // Get sales including items (filtered by tenant)
            var sales = await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .ToListAsync();

            // Calculate net profit
            decimal netProfit = 0;
            foreach (var sale in sales)
            {
                if (sale.SaleItems != null)
                {
                    foreach (var item in sale.SaleItems)
                    {
                        var purchasePrice = item.Product?.PurchasePrice ?? 0;
                        var salePrice = item.UnitPrice;
                        var quantity = item.Quantity;
                        netProfit += (salePrice - purchasePrice) * quantity;
                    }
                }
            }
            statistics.NetProfit = netProfit;

            // Period-based statistics
            statistics.PeriodSales = await salesQuery.CountAsync();
            statistics.PeriodRevenue = await salesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            // Recent sales (filtered by tenant)
            statistics.RecentSales = await _context.Sales
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Customer)
                .OrderByDescending(s => s.SaleDate)
                .Take(10)
                .ToListAsync();

            return statistics;
        }

        public async Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync(int year)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var monthlyRevenue = new List<MonthlyRevenueViewModel>();

            var arabicMonths = new[]
            {
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
    };

            for (int month = 1; month <= 12; month++)
            {
                var monthName = arabicMonths[month - 1];

                // ✅ نفلتر على TenantId الخاص بالمستأجر الحالي
                var revenue = await _context.Sales
                    .Where(s =>
                        s.TenantId == tenantId &&
                        s.SaleDate.Year == year &&
                        s.SaleDate.Month == month)
                    .SumAsync(s => (decimal?)s.TotalAmount) ?? 0; // ✅ تفادي Null

                monthlyRevenue.Add(new MonthlyRevenueViewModel
                {
                    Month = monthName,
                    MonthNumber = month,
                    Revenue = revenue,
                    TotalRevenue = revenue
                });
            }

            return monthlyRevenue;
        }


        public async Task<List<MonthlySalesViewModel>> GetMonthlySalesAsync(int year)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var monthlySales = new List<MonthlySalesViewModel>();

            var arabicMonths = new[]
            {
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
    };

            for (int month = 1; month <= 12; month++)
            {
                var monthName = arabicMonths[month - 1];

                // ✅ نضيف فلترة على TenantId
                var salesCount = await _context.Sales
                    .CountAsync(s =>
                        s.TenantId == tenantId &&
                        s.SaleDate.Year == year &&
                        s.SaleDate.Month == month);

                monthlySales.Add(new MonthlySalesViewModel
                {
                    Month = monthName,
                    MonthNumber = month,
                    SalesCount = salesCount,
                    TotalTransactions = salesCount
                });
            }

            return monthlySales;
        }

        public async Task<ComprehensiveReportViewModel> GetComprehensiveReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            var report = new ComprehensiveReportViewModel { FromDate = fromDate, ToDate = toDate };
            var currentYear = DateTime.Now.Year;

            await PopulateInventoryDataAsync(report);
            await PopulateSalesDataAsync(report, fromDate, toDate);
            await PopulateCustomersDataAsync(report, fromDate, toDate);

            report.MonthlyRevenue = await GetMonthlyRevenueAsync(currentYear);
            report.MonthlySales = await GetMonthlySalesAsync(currentYear);

            // Calculate final totals
            report.TotalRevenue = report.TotalSalesRevenue;

            return report;
        }

        private async Task PopulateInventoryDataAsync(ComprehensiveReportViewModel report)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();

            // ✅ نفلتر المنتجات بناءً على TenantId
            report.Products = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .AsNoTracking() // تحسين الأداء
                .ToListAsync();

            report.TotalProducts = report.Products.Sum(p => p.Quantity);
            report.LowStockProducts = report.Products.Count(p => p.Quantity <= 5);
            report.InventoryValue = report.Products.Sum(p => p.PurchasePrice * p.Quantity);
        }


        private async Task PopulateSalesDataAsync(ComprehensiveReportViewModel report, DateTime? fromDate, DateTime? toDate)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();

            // ✅ نفلتر المبيعات على المستأجر الحالي
            var salesQuery = _context.Sales
                .Where(s => s.TenantId == tenantId)
                .AsQueryable();

            // ✅ فلترة بالتاريخ
            if (fromDate.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);

            if (toDate.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);

            // ✅ تحميل البيانات المطلوبة فقط للمستخدم الحالي
            report.Sales = await salesQuery
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .AsNoTracking()
                .ToListAsync();

            // ✅ الحسابات بناءً على بيانات المستخدم الحالي فقط
            report.TotalSales = report.Sales.Count;
            report.TotalSalesRevenue = report.Sales.Sum(s => s.TotalAmount);
            report.NetSalesProfit = report.Sales
                .SelectMany(s => s.SaleItems)
                .Where(si => si.Product != null)
                .Sum(si => (si.UnitPrice - si.Product.PurchasePrice) * si.Quantity);
        }


        private async Task PopulateCustomersDataAsync(ComprehensiveReportViewModel report, DateTime? fromDate, DateTime? toDate)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();

            // ✅ نفلتر العملاء بناءً على المستأجر الحالي
            report.Customers = await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .Include(c => c.Sales)
                .AsNoTracking() // تحسين الأداء - عدم تتبع التغييرات
                .ToListAsync();

            report.TotalCustomers = report.Customers.Count;
        }


        // تنفيذ التقارير الجديدة
        public async Task<SalesReportViewModel> GetSalesReportAsync(DateTime? fromDate, DateTime? toDate, string? customerSearch, int? categoryId)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();

            var report = new SalesReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                CustomerId = null,
                CategoryId = categoryId,
                CustomerSearch = customerSearch
            };

            // ✅ فلترة المبيعات حسب المستأجر الحالي
            var salesQuery = _context.Sales
                .Where(s => s.TenantId == tenantId)
                .Include(s => s.Customer)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                    .ThenInclude(p => p.Category)
                .AsQueryable();

            // ✅ تطبيق فلاتر التاريخ
            if (fromDate.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);

            // ✅ فلترة بالعميل (بحث بالاسم)
            if (!string.IsNullOrEmpty(customerSearch))
                salesQuery = salesQuery.Where(s => s.Customer.FullName.Contains(customerSearch));

            // ✅ فلترة بالفئة (المنتجات داخل المبيعات)
            if (categoryId.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleItems.Any(si => si.Product.CategoryId == categoryId.Value));

            var sales = await salesQuery.AsNoTracking().ToListAsync();

            // ✅ حساب الإحصائيات
            report.TotalSales = sales.Count;
            report.TotalRevenue = sales.Sum(s => s.TotalAmount);
            report.TotalDiscount = sales.Sum(s => s.DiscountAmount);
            report.NetRevenue = report.TotalRevenue - report.TotalDiscount;
            report.TotalItems = sales.Sum(s => s.SaleItems.Count);
            report.AverageSaleValue = report.TotalSales > 0 ? report.TotalRevenue / report.TotalSales : 0;

            // ✅ حساب التكلفة والربح
            decimal totalCost = 0;
            foreach (var sale in sales)
            {
                foreach (var item in sale.SaleItems)
                {
                    totalCost += (item.Product?.PurchasePrice ?? 0) * item.Quantity;
                }
            }
            report.TotalCost = totalCost;
            report.GrossProfit = report.NetRevenue - report.TotalCost;
            report.ProfitMargin = report.NetRevenue > 0 ? (report.GrossProfit / report.NetRevenue) * 100 : 0;

            report.Customers = await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .OrderBy(c => c.FullName)
                .ToListAsync();

            report.Categories = await _context.Categories
                .Where(c => c.TenantId == tenantId && c.IsActive)
                .ToListAsync();

            // ✅ تعيين أسماء العميل والفئة
            if (!string.IsNullOrEmpty(customerSearch))
            {
                report.CustomerName = customerSearch;
            }

            if (categoryId.HasValue)
            {
                var category = report.Categories.FirstOrDefault(c => c.Id == categoryId.Value);
                report.CategoryName = category?.Name;
            }

            // ✅ إحصائيات يومية بناءً على مبيعات المستخدم الحالي فقط
            var dailySales = sales
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new DailySalesViewModel
                {
                    Date = g.Key,
                    SalesCount = g.Count(),
                    Revenue = g.Sum(s => s.TotalAmount),
                    Profit = g.Sum(s => s.SaleItems.Sum(si => (si.UnitPrice - (si.Product?.PurchasePrice ?? 0)) * si.Quantity))
                })
                .OrderBy(d => d.Date)
                .ToList();

            report.DailySales = dailySales;
            report.Sales = sales;

            return report;
        }


        public async Task<InventoryReportViewModel> GetInventoryReportAsync(int? categoryId, bool? lowStockOnly)
        {
            var report = new InventoryReportViewModel
            {
                CategoryId = categoryId,
                LowStockOnly = lowStockOnly
            };

            var tenantId = await _userService.GetCurrentTenantIdAsync();

            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Where(p => p.TenantId == tenantId) // 🔹 فلترة على المستأجر
                .AsQueryable();

            // تطبيق الفلاتر
            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);

            var inventorySettings = await _systemSettingsService.GetInventorySettingsAsync();

            if (lowStockOnly == true)
                productsQuery = productsQuery.Where(p => p.Quantity <= inventorySettings.LowStockThreshold);

            var products = await productsQuery.AsNoTracking().ToListAsync();

            // حساب الإحصائيات
            report.TotalProducts = products.Count;
            report.LowStockProducts = products.Count(p => p.Quantity <= inventorySettings.LowStockThreshold);
            report.OutOfStockProducts = products.Count(p => p.Quantity == 0);
            report.TotalInventoryValue = products.Sum(p => p.PurchasePrice * p.Quantity);
            report.AverageProductValue = report.TotalProducts > 0 ? report.TotalInventoryValue / report.TotalProducts : 0;

            // 🔹 إحصائيات حسب الفئة (مع فلترة المستأجر)
            var categoryInventory = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.TenantId == tenantId) // 🔹 فلترة على المستأجر
                .GroupBy(p => new { p.CategoryId, CategoryName = p.Category.Name })
                .Select(g => new CategoryInventoryViewModel
                {
                    CategoryId = g.Key.CategoryId ?? 0,
                    CategoryName = g.Key.CategoryName ?? "بدون فئة",
                    ProductCount = g.Count(),
                    TotalQuantity = g.Sum(p => p.Quantity),
                    TotalValue = g.Sum(p => p.PurchasePrice * p.Quantity),
                    LowStockCount = g.Count(p => p.Quantity <= inventorySettings.LowStockThreshold)
                })
                .AsNoTracking()
                .ToListAsync();

            report.CategoryInventory = categoryInventory;

            report.Categories = await _context.Categories
                .Where(c => c.IsActive && c.TenantId == tenantId) // 🔹 فلترة الفئات برضو
                .AsNoTracking()
                .ToListAsync();

            report.Products = products;

            if (categoryId.HasValue)
            {
                var category = report.Categories.FirstOrDefault(c => c.Id == categoryId.Value);
                report.CategoryName = category?.Name;
            }

            return report;
        }


        public async Task<CustomerReportViewModel> GetCustomerReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            var report = new CustomerReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var tenantId = await _userService.GetCurrentTenantIdAsync();

            // 🔹 استعلام العملاء الأساسي مع فلترة المستأجر
            var customersQuery = _context.Customers
                .Include(c => c.Sales)
                .ThenInclude(s => s.SaleItems)
                .Where(c => c.TenantId == tenantId) // فلترة العملاء حسب المستأجر
                .AsQueryable();

            var customers = await customersQuery
                .AsNoTracking() // تحسين الأداء
                .ToListAsync();

            // حساب الإحصائيات
            report.TotalCustomers = customers.Count;
            report.ActiveCustomers = customers.Count(c => c.Sales.Any());
            report.TotalCustomerRevenue = customers.Sum(c => c.Sales.Sum(s => s.TotalAmount));
            report.AverageCustomerValue = report.ActiveCustomers > 0 ? report.TotalCustomerRevenue / report.ActiveCustomers : 0;

            // 🔹 العملاء الجدد بناءً على التاريخ مع فلترة المستأجر
            var newCustomersQuery = _context.Customers
                .Include(c => c.Sales)
                .Where(c => c.TenantId == tenantId)
                .AsQueryable();

            if (fromDate.HasValue)
                newCustomersQuery = newCustomersQuery.Where(c => c.Sales.Any(s => s.SaleDate >= fromDate.Value));

            if (toDate.HasValue)
                newCustomersQuery = newCustomersQuery.Where(c => c.Sales.Any(s => s.SaleDate <= toDate.Value));

            report.NewCustomers = await newCustomersQuery.CountAsync();

            // 🔹 أفضل العملاء
            var topCustomers = customers
                .Where(c => c.Sales.Any())
                .Select(c => new TopCustomerViewModel
                {
                    CustomerId = c.Id,
                    CustomerName = c.FullName,
                    SalesCount = c.Sales.Count,
                    TotalSpent = c.Sales.Sum(s => s.TotalAmount)
                })
                .OrderByDescending(tc => tc.TotalSpent)
                .Take(10)
                .ToList();

            report.TopCustomers = topCustomers;
            report.Customers = customers;

            return report;
        }


        public async Task<FinancialReportViewModel> GetFinancialReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            var report = new FinancialReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var tenantId = await _userService.GetCurrentTenantIdAsync();

            // 🔹 فلترة المعاملات الخاصة بالمستأجر فقط
            var transactionsQuery = _context.StoreAccounts
                .Where(t => t.TenantId == tenantId)
                .AsQueryable();

            if (fromDate.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.TransactionDate <= toDate.Value);

            var transactions = await transactionsQuery
                .AsNoTracking() // تحسين الأداء - عدم تتبع الكيانات
                .ToListAsync();

            // 🔹 حساب الإحصائيات المالية
            report.TotalIncome = transactions
                .Where(t => t.TransactionType == TransactionType.Income)
                .Sum(t => t.Amount);

            report.TotalExpenses = transactions
                .Where(t => t.TransactionType == TransactionType.Expense)
                .Sum(t => t.Amount);

            report.NetProfit = report.TotalIncome - report.TotalExpenses;
            report.CashBalance = report.NetProfit;
            report.TotalTransactions = transactions.Count;

            // 🔹 حساب الأصول (المخزون + الرصيد النقدي)
            var inventoryValue = await _context.Products
                .Where(p => p.TenantId == tenantId) // فلترة المنتجات حسب المستأجر
                .SumAsync(p => p.PurchasePrice * p.Quantity);

            report.TotalAssets = inventoryValue + report.CashBalance;

            // 🔹 حساب الخصوم (الديون العامة)
            var totalGeneralDebts = await _context.GeneralDebts
                .Where(gd => gd.TenantId == tenantId) // فلترة الديون حسب المستأجر
                .SumAsync(gd => gd.Amount - gd.PaidAmount);

            report.TotalLiabilities = totalGeneralDebts;

            // 🔹 حساب رأس المال العامل
            report.WorkingCapital = report.TotalAssets - report.TotalLiabilities;

            report.Transactions = transactions;

            return report;
        }
        public async Task<GeneralDebtReportViewModel> GetGeneralDebtReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            var report = new GeneralDebtReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var debtsQuery = _context.GeneralDebts
                .Where(gd => gd.TenantId == tenantId) // 🔹 فلترة حسب المستأجر الحالي
                .AsQueryable();

            if (fromDate.HasValue)
                debtsQuery = debtsQuery.Where(gd => gd.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                debtsQuery = debtsQuery.Where(gd => gd.CreatedAt <= toDate.Value);

            var debts = await debtsQuery.ToListAsync();

            // حساب الإحصائيات
            report.TotalDebts = debts.Count;
            report.TotalDebtAmount = debts.Sum(gd => gd.Amount);
            report.TotalPaidAmount = debts.Sum(gd => gd.PaidAmount);
            report.OutstandingAmount = report.TotalDebtAmount - report.TotalPaidAmount;
            report.ActiveDebts = debts.Count(gd => (gd.Amount - gd.PaidAmount) > 0);
            report.SettledDebts = debts.Count(gd => (gd.Amount - gd.PaidAmount) == 0);

            // حالة الديون
            var debtStatus = new List<DebtStatusViewModel>
    {
        new DebtStatusViewModel { Status = "نشطة", Count = report.ActiveDebts, TotalAmount = debts.Where(gd => (gd.Amount - gd.PaidAmount) > 0).Sum(gd => gd.Amount - gd.PaidAmount) },
        new DebtStatusViewModel { Status = "مستوفاة", Count = report.SettledDebts, TotalAmount = report.TotalPaidAmount }
    };

            report.DebtStatus = debtStatus;
            report.Debts = debts;

            return report;
        }


        public async Task<StoreAccountReportViewModel> GetStoreAccountReportAsync(DateTime? fromDate, DateTime? toDate, string? transactionType)
        {
            var report = new StoreAccountReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                TransactionType = transactionType
            };

            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var transactionsQuery = _context.StoreAccounts
                .Where(sa => sa.TenantId == tenantId) // 🔹 فلترة حسب المستأجر الحالي
                .AsQueryable();

            if (fromDate.HasValue)
                transactionsQuery = transactionsQuery.Where(sa => sa.TransactionDate >= fromDate.Value);
            if (toDate.HasValue)
                transactionsQuery = transactionsQuery.Where(sa => sa.TransactionDate <= toDate.Value);
            if (!string.IsNullOrEmpty(transactionType))
                transactionsQuery = transactionsQuery.Where(sa => sa.TransactionType.ToString() == transactionType);

            var transactions = await transactionsQuery.ToListAsync();

            // حساب الإحصائيات
            report.TotalIncome = transactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount);
            report.TotalExpenses = transactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount);
            report.NetBalance = report.TotalIncome - report.TotalExpenses;
            report.TotalTransactions = transactions.Count;
            report.AverageTransactionValue = report.TotalTransactions > 0 ? (report.TotalIncome + report.TotalExpenses) / report.TotalTransactions : 0;

            // إحصائيات حسب نوع العملية
            var transactionTypes = transactions
                .GroupBy(t => t.TransactionType)
                .Select(g => new TransactionTypeViewModel
                {
                    TransactionType = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .ToList();

            // إحصائيات حسب طريقة الدفع
            var paymentMethods = transactions
                .GroupBy(t => t.PaymentMethodId)
                .Select(g => new PaymentMethodViewModel
                {
                    PaymentMethod = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(x => x.Amount)
                })
                .ToList();

            report.TransactionTypes = transactionTypes;
            report.PaymentMethods = paymentMethods;
            report.Transactions = transactions;

            return report;
        }


        public async Task<ProfitLossReportViewModel> GetProfitLossReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            var report = new ProfitLossReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            // 🔹 حساب إيرادات المبيعات
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var salesQuery = _context.Sales
                .Where(s => s.TenantId == tenantId) // فلترة بالمستأجر الحالي
                .AsQueryable();

            if (fromDate.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);

            var sales = await salesQuery
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ToListAsync();

            report.SalesRevenue = sales.Sum(s => s.TotalAmount);

            // 🔹 حساب إيرادات أخرى
            var otherIncome = await _context.StoreAccounts
                .Where(sa => sa.TenantId == tenantId && sa.TransactionType == TransactionType.Income)
                .SumAsync(sa => sa.Amount);
            report.OtherIncome = otherIncome;
            report.TotalRevenue = report.SalesRevenue + report.OtherIncome;

            // 🔹 حساب تكلفة البضائع المباعة
            decimal costOfGoodsSold = 0;
            foreach (var sale in sales)
            {
                foreach (var item in sale.SaleItems)
                {
                    costOfGoodsSold += (item.Product?.PurchasePrice ?? 0) * item.Quantity;
                }
            }
            report.CostOfGoodsSold = costOfGoodsSold;

            // 🔹 حساب المصروفات التشغيلية
            var operatingExpenses = await _context.StoreAccounts
                .Where(sa => sa.TenantId == tenantId && sa.TransactionType == TransactionType.Expense)
                .SumAsync(sa => sa.Amount);
            report.OperatingExpenses = operatingExpenses;

            report.TotalExpenses = report.CostOfGoodsSold + report.OperatingExpenses;

            // 🔹 حساب النتائج
            report.GrossProfit = report.SalesRevenue - report.CostOfGoodsSold;
            report.OperatingProfit = report.GrossProfit - report.OperatingExpenses;
            report.NetProfit = report.TotalRevenue - report.TotalExpenses;
            report.ProfitMargin = report.TotalRevenue > 0 ? (report.NetProfit / report.TotalRevenue) * 100 : 0;

            return report;
        }

        public async Task<CategoryPerformanceReportViewModel> GetCategoryPerformanceReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            var report = new CategoryPerformanceReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var salesQuery = _context.Sales
                .Where(s => s.TenantId == tenantId) // فلترة بالمستأجر الحالي
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ThenInclude(p => p.Category)
                .AsQueryable();

            if (fromDate.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);

            var sales = await salesQuery.ToListAsync();

            // حساب أداء الفئات
            var categoryPerformance = sales
                .SelectMany(s => s.SaleItems)
                .Where(si => si.Product?.Category != null)
                .GroupBy(si => new { si.Product.Category.Id, si.Product.Category.Name })
                .Select(g => new CategoryPerformanceViewModel
                {
                    CategoryId = g.Key.Id,
                    CategoryName = g.Key.Name,
                    SalesCount = g.Count(),
                    Revenue = g.Sum(si => si.UnitPrice * si.Quantity),
                    Profit = g.Sum(si => (si.UnitPrice - (si.Product.PurchasePrice)) * si.Quantity)
                })
                .ToList();

            foreach (var cp in categoryPerformance)
            {
                cp.ProfitMargin = cp.Revenue > 0 ? (cp.Profit / cp.Revenue) * 100 : 0;
            }

            report.CategoryPerformance = categoryPerformance.OrderByDescending(cp => cp.Revenue).ToList();
            report.Categories = await _context.Categories
                .Where(c => c.IsActive && c.TenantId == tenantId) // فلترة بالفئة الخاصة بالمستأجر
                .ToListAsync();

            report.TotalCategories = report.Categories.Count;
            report.TotalRevenue = categoryPerformance.Sum(cp => cp.Revenue);
            report.TotalProfit = categoryPerformance.Sum(cp => cp.Profit);
            report.TotalSales = sales.Count;

            return report;
        }


        // Keep these for chart compatibility
        public class MonthlyRevenueViewModel
        {
            public string Month { get; set; }
            public int MonthNumber { get; set; }
            public decimal Revenue { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        public class MonthlySalesViewModel
        {
            public string Month { get; set; }
            public int MonthNumber { get; set; }
            public int SalesCount { get; set; }
            public int TotalTransactions { get; set; }
        }
    }
}
