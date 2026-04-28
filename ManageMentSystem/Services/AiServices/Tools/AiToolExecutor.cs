using ManageMentSystem.Data;
using ManageMentSystem.Services.UserServices;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ManageMentSystem.Services.AiServices
{
    public class AiToolExecutor : IAiToolExecutor
    {
        private const int NormalizedSearchCandidateLimit = 400;
        private static readonly HashSet<string> AllowedPeriods = new(StringComparer.OrdinalIgnoreCase)
        {
            "today", "week", "month", "year", "all"
        };

        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IAiTelemetryService _telemetry;
        private string? _cachedTenantId;

        public AiToolExecutor(
            AppDbContext context,
            IUserService userService,
            IConfiguration configuration,
            IAiTelemetryService telemetry)
        {
            _context = context;
            _userService = userService;
            _configuration = configuration;
            _telemetry = telemetry;
        }

        public async Task<object> ExecuteAsync(string functionName, IDictionary<string, object> args)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                return ErrorEnvelope("invalid_function", "Function name is required.", functionName ?? string.Empty, 0);
            }

            var timeoutMs = ParsePositiveInt(_configuration["AI:Tools:TimeoutMs"], 10000);
            var retries = ParseNonNegativeInt(_configuration["AI:Tools:Retries"], 1);
            var maxAttempts = retries + 1;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var startedAt = DateTime.UtcNow;
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var task = ExecuteCoreAsync(functionName, args);
                    var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));

                    if (completed != task)
                    {
                        if (attempt == maxAttempts)
                        {
                            stopwatch.Stop();
                            _telemetry.TrackToolExecution(functionName, false, attempt, stopwatch.ElapsedMilliseconds, "timeout");
                            return ErrorEnvelope("timeout", $"Tool execution timed out after {timeoutMs}ms.", functionName, attempt);
                        }

                        stopwatch.Stop();
                        _telemetry.TrackToolExecution(functionName, false, attempt, stopwatch.ElapsedMilliseconds, "timeout_retry");
                        continue;
                    }

                    var data = await task;
                    stopwatch.Stop();
                    _telemetry.TrackToolExecution(functionName, true, attempt, stopwatch.ElapsedMilliseconds);
                    return SuccessEnvelope(functionName, data, attempt, startedAt);
                }
                catch (ArgumentException ex)
                {
                    stopwatch.Stop();
                    _telemetry.TrackToolExecution(functionName, false, attempt, stopwatch.ElapsedMilliseconds, "validation_error");
                    return ErrorEnvelope("validation_error", ex.Message, functionName, attempt);
                }
                catch (Exception ex)
                {
                    if (attempt == maxAttempts)
                    {
                        stopwatch.Stop();
                        _telemetry.TrackToolExecution(functionName, false, attempt, stopwatch.ElapsedMilliseconds, "execution_error");
                        return ErrorEnvelope("execution_error", ex.Message, functionName, attempt);
                    }

                    stopwatch.Stop();
                    _telemetry.TrackToolExecution(functionName, false, attempt, stopwatch.ElapsedMilliseconds, "execution_retry");
                }
            }

            return ErrorEnvelope("execution_error", "Unknown tool execution failure.", functionName, maxAttempts);
        }

        private async Task<object> ExecuteCoreAsync(string functionName, IDictionary<string, object> args)
        {
            return functionName switch
            {
                "get_total_sales" => await GetTotalSalesAsync(args),
                "get_top_products" => await GetTopProductsAsync(args),
                "get_monthly_sales" => await GetMonthlySalesAsync(args),
                "get_profit" => await GetProfitAsync(args),
                "get_low_stock_products" => await GetLowStockProductsAsync(args),
                "get_top_customers" => await GetTopCustomersAsync(args),
                "get_store_account_summary" => await GetStoreAccountSummaryAsync(args),
                "get_pending_debts" => await GetPendingDebtsAsync(),
                "get_general_statistics" => await GetGeneralStatisticsAsync(args),
                "get_sales_report" => await GetSalesReportAsync(args),
                "get_inventory_report" => await GetInventoryReportAsync(args),
                "get_customer_report" => await GetCustomerReportAsync(args),
                "get_financial_report" => await GetFinancialReportAsync(args),
                "get_general_debt_report" => await GetGeneralDebtReportAsync(args),
                "get_category_performance_report" => await GetCategoryPerformanceReportAsync(args),
                "get_installments_summary" => await GetInstallmentsSummaryAsync(args),
                "get_payment_methods_summary" => await GetPaymentMethodsSummaryAsync(args),
                "get_customer_info" => await GetCustomerInfoAsync(args),
                "search_product" => await SearchProductAsync(args),
                "get_expense_details" => await GetExpenseDetailsAsync(args),
                "get_customer_account_statement" => await GetCustomerAccountStatementAsync(args),
                "get_store_transactions" => await GetStoreTransactionsAsync(args),
                "get_sales_details" => await GetSalesDetailsAsync(args),
                _ => throw new ArgumentException($"Unknown function '{functionName}'.")
            };
        }

        private async Task<object> GetTotalSalesAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");

            EnsureDateRangeIsValid(fromDate, toDate);

            var query = _context.Sales.Where(s => s.TenantId == tenantId);
            if (fromDate.HasValue) query = query.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(s => s.SaleDate <= toDate.Value);

            var totalAmount = await query.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            var count = await query.CountAsync();

            return new
            {
                total_amount = Math.Round(totalAmount, 2),
                sales_count = count,
                from_date = fromDate?.ToString("yyyy-MM-dd") ?? "start",
                to_date = toDate?.ToString("yyyy-MM-dd") ?? "today",
                currency = "EGP"
            };
        }

        private async Task<object> GetTopProductsAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var topN = Clamp(ParseInt(args, "top_n", 5), 1, 20);
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");

            EnsureDateRangeIsValid(fromDate, toDate);

            var query = _context.SaleItems
                .Include(si => si.Product)
                .Include(si => si.Sale)
                .Where(si => si.Sale.TenantId == tenantId && si.Product != null);

            if (fromDate.HasValue) query = query.Where(si => si.Sale.SaleDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(si => si.Sale.SaleDate <= toDate.Value);

            var topProducts = await query
                .GroupBy(si => new { si.ProductId, si.Product.Name, si.Product.SalePrice })
                .Select(g => new
                {
                    product_name = g.Key.Name,
                    total_quantity = g.Sum(si => si.Quantity),
                    total_revenue = g.Sum(si => si.UnitPrice * si.Quantity),
                    sale_price = g.Key.SalePrice
                })
                .OrderByDescending(p => p.total_quantity)
                .Take(topN)
                .ToListAsync();

            return new { top_products = topProducts, count = topProducts.Count };
        }

        private async Task<object> GetMonthlySalesAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var year = Clamp(ParseInt(args, "year", DateTime.Now.Year), 2020, DateTime.Now.Year + 1);
            var monthly = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.SaleDate.Year == year)
                .GroupBy(s => s.SaleDate.Month)
                .Select(g => new { month = g.Key, revenue = g.Sum(x => x.TotalAmount) })
                .ToListAsync();

            return new
            {
                year,
                monthly_data = Enumerable.Range(1, 12)
                    .Select(m => new
                    {
                        month = m,
                        revenue = Math.Round(monthly.FirstOrDefault(x => x.month == m)?.revenue ?? 0m, 2)
                    })
                    .ToList()
            };
        }

        private async Task<object> GetProfitAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");

            EnsureDateRangeIsValid(fromDate, toDate);

            var salesQuery = _context.Sales
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId);

            if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);

            var salesRevenue = await salesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
            var costOfGoodsQuery = _context.SaleItems
                .AsNoTracking()
                .Where(si => si.Sale.TenantId == tenantId);
            if (fromDate.HasValue) costOfGoodsQuery = costOfGoodsQuery.Where(si => si.Sale.SaleDate >= fromDate.Value);
            if (toDate.HasValue) costOfGoodsQuery = costOfGoodsQuery.Where(si => si.Sale.SaleDate <= toDate.Value);
            var costOfGoods = await costOfGoodsQuery
                .SumAsync(si => (decimal?)((si.Product != null ? si.Product.PurchasePrice : 0m) * si.Quantity)) ?? 0m;
            var grossProfit = salesRevenue - costOfGoods;

            var expenseQuery = _context.StoreAccounts
                .AsNoTracking()
                .Where(sa => sa.TenantId == tenantId && sa.TransactionType == Models.TransactionType.Expense);

            if (fromDate.HasValue) expenseQuery = expenseQuery.Where(sa => sa.TransactionDate >= fromDate.Value);
            if (toDate.HasValue) expenseQuery = expenseQuery.Where(sa => sa.TransactionDate <= toDate.Value);

            var operatingExpenses = await expenseQuery.SumAsync(sa => (decimal?)sa.Amount) ?? 0m;
            var netProfit = grossProfit - operatingExpenses;
            var margin = salesRevenue > 0 ? (netProfit / salesRevenue) * 100m : 0m;

            return new
            {
                sales_revenue = Math.Round(salesRevenue, 2),
                cost_of_goods = Math.Round(costOfGoods, 2),
                gross_profit = Math.Round(grossProfit, 2),
                operating_expenses = Math.Round(operatingExpenses, 2),
                net_profit = Math.Round(netProfit, 2),
                profit_margin_pct = Math.Round(margin, 2),
                currency = "EGP"
            };
        }

        private async Task<object> GetLowStockProductsAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var threshold = Clamp(ParseInt(args, "threshold", 5), 0, 1000);

            var products = await _context.Products
                .Where(p => p.TenantId == tenantId && p.Quantity <= threshold)
                .Include(p => p.Category)
                .OrderBy(p => p.Quantity)
                .Select(p => new
                {
                    name = p.Name,
                    current_stock = p.Quantity,
                    category = p.Category != null ? p.Category.Name : "Uncategorized",
                    sale_price = p.SalePrice
                })
                .ToListAsync();

            return new { low_stock_products = products, count = products.Count, threshold };
        }

        private async Task<object> GetTopCustomersAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var topN = Clamp(ParseInt(args, "top_n", 5), 1, 20);
            var top = await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.TenantId == tenantId && s.CustomerId != null && s.Customer != null)
                .GroupBy(s => new { s.CustomerId, s.Customer.FullName })
                .Select(g => new
                {
                    name = g.Key.FullName,
                    total_spent = Math.Round(g.Sum(x => x.TotalAmount), 2),
                    purchases_count = g.Count()
                })
                .OrderByDescending(x => x.total_spent)
                .Take(topN)
                .ToListAsync();

            return new { top_customers = top, count = top.Count, currency = "EGP" };
        }

        private async Task<object> GetStoreAccountSummaryAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");

            EnsureDateRangeIsValid(fromDate, toDate);

            var query = _context.StoreAccounts.AsNoTracking().Where(sa => sa.TenantId == tenantId);
            if (fromDate.HasValue) query = query.Where(sa => sa.TransactionDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(sa => sa.TransactionDate <= toDate.Value);

            var transactions = await query.ToListAsync();
            var income = transactions.Where(t => t.TransactionType == Models.TransactionType.Income).Sum(t => t.Amount);
            var expenses = transactions.Where(t => t.TransactionType == Models.TransactionType.Expense).Sum(t => t.Amount);
            var net = income - expenses;

            return new
            {
                total_income = Math.Round(income, 2),
                total_expenses = Math.Round(expenses, 2),
                net_balance = Math.Round(net, 2),
                transactions_count = transactions.Count,
                currency = "EGP"
            };
        }

        private async Task<object> GetPendingDebtsAsync()
        {
            var tenantId = await RequireTenantIdAsync();

            var generalDebts = await _context.GeneralDebts
                .Where(gd => gd.TenantId == tenantId)
                .SumAsync(gd => gd.Amount - gd.PaidAmount);

            var installmentsDebt = await _context.Installments
                .Where(i => i.TenantId == tenantId)
                .SumAsync(i => (decimal?)(i.TotalAmount - i.TotalPaid)) ?? 0;

            var customerDebts = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.PaidAmount < s.TotalAmount)
                .SumAsync(s => (decimal?)(s.TotalAmount - s.PaidAmount)) ?? 0;

            return new
            {
                general_debts = Math.Round(generalDebts, 2),
                installments_remaining = Math.Round(installmentsDebt, 2),
                customer_debts = Math.Round(customerDebts, 2),
                total_pending = Math.Round(generalDebts + installmentsDebt + customerDebts, 2),
                currency = "EGP"
            };
        }

        private async Task<object> GetGeneralStatisticsAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var period = args.TryGetValue("period", out var value) ? value?.ToString() ?? "all" : "all";
            period = period.ToLowerInvariant();

            if (!AllowedPeriods.Contains(period))
            {
                throw new ArgumentException("Invalid period. Allowed: today, week, month, year, all.");
            }

            var (periodStart, periodEnd) = GetPeriodRange(period);

            var salesQuery = _context.Sales.AsNoTracking().Where(s => s.TenantId == tenantId);
            if (periodStart.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= periodStart.Value);
            if (periodEnd.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= periodEnd.Value);

            var salesCount = await salesQuery.CountAsync();
            var totalRevenue = await salesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
            var costOfGoodsQuery = _context.SaleItems
                .AsNoTracking()
                .Where(si => si.Sale.TenantId == tenantId);
            if (periodStart.HasValue) costOfGoodsQuery = costOfGoodsQuery.Where(si => si.Sale.SaleDate >= periodStart.Value);
            if (periodEnd.HasValue) costOfGoodsQuery = costOfGoodsQuery.Where(si => si.Sale.SaleDate <= periodEnd.Value);
            var costOfGoods = await costOfGoodsQuery
                .SumAsync(si => (decimal?)((si.Product != null ? si.Product.PurchasePrice : 0m) * si.Quantity)) ?? 0m;
            var expensesQuery = _context.StoreAccounts
                .AsNoTracking()
                .Where(sa => sa.TenantId == tenantId && sa.TransactionType == Models.TransactionType.Expense);
            if (periodStart.HasValue) expensesQuery = expensesQuery.Where(sa => sa.TransactionDate >= periodStart.Value);
            if (periodEnd.HasValue) expensesQuery = expensesQuery.Where(sa => sa.TransactionDate <= periodEnd.Value);
            var expenses = await expensesQuery.SumAsync(sa => (decimal?)sa.Amount) ?? 0m;

            var totalCustomers = await _context.Customers.CountAsync(c => c.TenantId == tenantId);
            var totalInventory = await _context.Products.Where(p => p.TenantId == tenantId).SumAsync(p => (int?)p.Quantity) ?? 0;
            var lowStockCount = await _context.Products.CountAsync(p => p.TenantId == tenantId && p.Quantity <= 5);

            return new
            {
                total_sales_count = salesCount,
                total_revenue = Math.Round(totalRevenue, 2),
                total_customers = totalCustomers,
                total_inventory = totalInventory,
                net_profit = Math.Round(totalRevenue - costOfGoods - expenses, 2),
                low_stock_count = lowStockCount,
                period,
                currency = "EGP"
            };
        }

        private async Task<object> GetSalesReportAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");
            var customerId = ParseNullableInt(args, "customer_id");
            var categoryId = ParseNullableInt(args, "category_id");

            EnsureDateRangeIsValid(fromDate, toDate);

            var salesQuery = _context.Sales
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId);

            if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);
            if (customerId.HasValue) salesQuery = salesQuery.Where(s => s.CustomerId == customerId.Value);
            if (categoryId.HasValue) salesQuery = salesQuery.Where(s => s.SaleItems.Any(si => si.Product != null && si.Product.CategoryId == categoryId.Value));

            var totalSales = await salesQuery.CountAsync();
            var totalRevenue = await salesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
            var totalDiscount = await salesQuery.SumAsync(s => (decimal?)s.DiscountAmount) ?? 0m;
            var netRevenue = totalRevenue - totalDiscount;

            var saleItemsQuery = _context.SaleItems
                .AsNoTracking()
                .Where(si => si.Sale.TenantId == tenantId);
            if (fromDate.HasValue) saleItemsQuery = saleItemsQuery.Where(si => si.Sale.SaleDate >= fromDate.Value);
            if (toDate.HasValue) saleItemsQuery = saleItemsQuery.Where(si => si.Sale.SaleDate <= toDate.Value);
            if (customerId.HasValue) saleItemsQuery = saleItemsQuery.Where(si => si.Sale.CustomerId == customerId.Value);
            if (categoryId.HasValue) saleItemsQuery = saleItemsQuery.Where(si => si.Product != null && si.Product.CategoryId == categoryId.Value);

            var totalCost = await saleItemsQuery
                .SumAsync(si => (decimal?)((si.Product != null ? si.Product.PurchasePrice : 0m) * si.Quantity)) ?? 0m;
            var grossProfit = netRevenue - totalCost;
            var profitMargin = netRevenue > 0 ? (grossProfit / netRevenue) * 100m : 0m;

            var dailySales = await salesQuery
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new { date = g.Key, count = g.Count(), revenue = g.Sum(x => x.TotalAmount) })
                .OrderByDescending(x => x.revenue)
                .Take(5)
                .ToListAsync();

            return new
            {
                total_sales = totalSales,
                total_revenue = Math.Round(totalRevenue, 2),
                total_discount = Math.Round(totalDiscount, 2),
                net_revenue = Math.Round(netRevenue, 2),
                total_cost = Math.Round(totalCost, 2),
                gross_profit = Math.Round(grossProfit, 2),
                profit_margin = Math.Round(profitMargin, 2),
                average_sale_value = Math.Round(totalSales > 0 ? totalRevenue / totalSales : 0m, 2),
                top_days = dailySales
                    .Select(d => new { date = d.date.ToString("yyyy-MM-dd"), sales_count = d.count, revenue = Math.Round(d.revenue, 2) })
                    .ToList(),
                currency = "EGP"
            };
        }

        private async Task<object> GetInventoryReportAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var categoryId = ParseNullableInt(args, "category_id");
            var lowStockOnly = ParseNullableBool(args, "low_stock_only");

            var productsQuery = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.TenantId == tenantId);

            if (categoryId.HasValue) productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            if (lowStockOnly == true) productsQuery = productsQuery.Where(p => p.Quantity <= 5);

            var products = await productsQuery.ToListAsync();
            var categoryInventory = products
                .GroupBy(p => p.Category != null ? p.Category.Name : "بدون فئة")
                .Select(g => new
                {
                    category = g.Key,
                    products = g.Count(),
                    total_value = g.Sum(x => x.PurchasePrice * x.Quantity),
                    low_stock = g.Count(x => x.Quantity <= 5)
                })
                .OrderByDescending(x => x.total_value)
                .Take(5)
                .ToList();

            return new
            {
                total_products = products.Count,
                low_stock_products = products.Count(p => p.Quantity <= 5),
                out_of_stock_products = products.Count(p => p.Quantity == 0),
                total_inventory_value = Math.Round(products.Sum(p => p.PurchasePrice * p.Quantity), 2),
                average_product_value = Math.Round(products.Count > 0 ? products.Sum(p => p.PurchasePrice * p.Quantity) / products.Count : 0m, 2),
                top_categories = categoryInventory.Select(c => new
                {
                    category = c.category,
                    products = c.products,
                    total_value = Math.Round(c.total_value, 2),
                    low_stock = c.low_stock
                }).ToList(),
                currency = "EGP"
            };
        }

        private async Task<object> GetCustomerReportAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");

            EnsureDateRangeIsValid(fromDate, toDate);

            var customersQuery = _context.Customers
                .AsNoTracking()
                .Where(c => c.TenantId == tenantId);

            var salesQuery = _context.Sales.AsNoTracking().Where(s => s.TenantId == tenantId);
            if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);
            var totalCustomers = await customersQuery.CountAsync();
            var activeCustomers = await _context.Sales
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId && s.CustomerId != null)
                .Select(s => s.CustomerId)
                .Distinct()
                .CountAsync();
            var newCustomersQuery = customersQuery;
            if (fromDate.HasValue) newCustomersQuery = newCustomersQuery.Where(c => c.CreatedAt >= fromDate.Value);
            if (toDate.HasValue) newCustomersQuery = newCustomersQuery.Where(c => c.CreatedAt <= toDate.Value);
            var newCustomers = await newCustomersQuery.CountAsync();
            var totalCustomerRevenue = await salesQuery.SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;

            var topCustomers = await salesQuery
                .Where(s => s.CustomerId != null && s.Customer != null)
                .GroupBy(s => s.Customer!.FullName)
                .Select(g => new { customer_name = g.Key, sales_count = g.Count(), total_spent = g.Sum(x => x.TotalAmount) })
                .OrderByDescending(x => x.total_spent)
                .Take(10)
                .ToListAsync();

            return new
            {
                total_customers = totalCustomers,
                active_customers = activeCustomers,
                new_customers = newCustomers,
                total_customer_revenue = Math.Round(totalCustomerRevenue, 2),
                average_customer_value = Math.Round(totalCustomers > 0 ? totalCustomerRevenue / totalCustomers : 0m, 2),
                top_customers = topCustomers.Select(c => new { c.customer_name, c.sales_count, total_spent = Math.Round(c.total_spent, 2) }).ToList(),
                currency = "EGP"
            };
        }

        private async Task<object> GetFinancialReportAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");

            EnsureDateRangeIsValid(fromDate, toDate);

            var transactionsQuery = _context.StoreAccounts.AsNoTracking().Where(sa => sa.TenantId == tenantId);
            if (fromDate.HasValue) transactionsQuery = transactionsQuery.Where(sa => sa.TransactionDate >= fromDate.Value);
            if (toDate.HasValue) transactionsQuery = transactionsQuery.Where(sa => sa.TransactionDate <= toDate.Value);
            var transactions = await transactionsQuery.ToListAsync();

            var income = transactions.Where(t => t.TransactionType == Models.TransactionType.Income).Sum(t => t.Amount);
            var expenses = transactions.Where(t => t.TransactionType == Models.TransactionType.Expense).Sum(t => t.Amount);
            var net = income - expenses;
            var inventoryValue = await _context.Products.Where(p => p.TenantId == tenantId).SumAsync(p => (decimal?)(p.PurchasePrice * p.Quantity)) ?? 0m;
            var liabilities = await _context.GeneralDebts.Where(g => g.TenantId == tenantId).SumAsync(g => (decimal?)(g.Amount - g.PaidAmount)) ?? 0m;

            return new
            {
                total_income = Math.Round(income, 2),
                total_expenses = Math.Round(expenses, 2),
                net_profit = Math.Round(net, 2),
                cash_balance = Math.Round(net, 2),
                total_assets = Math.Round(inventoryValue + net, 2),
                total_liabilities = Math.Round(liabilities, 2),
                working_capital = Math.Round((inventoryValue + net) - liabilities, 2),
                total_transactions = transactions.Count,
                currency = "EGP"
            };
        }

        private async Task<object> GetGeneralDebtReportAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");

            EnsureDateRangeIsValid(fromDate, toDate);

            var query = _context.GeneralDebts.AsNoTracking().Where(gd => gd.TenantId == tenantId);
            if (fromDate.HasValue) query = query.Where(gd => gd.CreatedAt >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(gd => gd.CreatedAt <= toDate.Value);
            var debts = await query.ToListAsync();

            var totalAmount = debts.Sum(d => d.Amount);
            var totalPaid = debts.Sum(d => d.PaidAmount);
            var outstanding = totalAmount - totalPaid;
            var active = debts.Count(d => (d.Amount - d.PaidAmount) > 0);
            var settled = debts.Count(d => (d.Amount - d.PaidAmount) <= 0);

            return new
            {
                total_debts = debts.Count,
                active_debts = active,
                settled_debts = settled,
                total_debt_amount = Math.Round(totalAmount, 2),
                total_paid_amount = Math.Round(totalPaid, 2),
                outstanding_amount = Math.Round(outstanding, 2),
                debt_status = debts
                    .GroupBy(d => d.DebtType.ToString())
                    .Select(g => new
                    {
                        status = g.Key,
                        count = g.Count(),
                        total_amount = Math.Round(g.Sum(x => x.Amount - x.PaidAmount), 2)
                    })
                    .ToList(),
                top_debts = debts
                    .OrderByDescending(d => d.Amount - d.PaidAmount)
                    .Take(10)
                    .Select(d => new
                {
                    title = d.Title,
                    party = d.PartyName,
                    debt_type = d.DebtType.ToString(),
                    remaining = Math.Round(d.Amount - d.PaidAmount, 2)
                }).ToList(),
                currency = "EGP"
            };
        }

        private async Task<object> GetCategoryPerformanceReportAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");

            EnsureDateRangeIsValid(fromDate, toDate);

            var salesQuery = _context.Sales.AsNoTracking().Where(s => s.TenantId == tenantId);
            if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);
            var totalSales = await salesQuery.CountAsync();

            var saleItems = await _context.SaleItems
                .AsNoTracking()
                .Include(si => si.Product)
                .ThenInclude(p => p.Category)
                .Where(si => si.Sale.TenantId == tenantId)
                .Where(si => !fromDate.HasValue || si.Sale.SaleDate >= fromDate.Value)
                .Where(si => !toDate.HasValue || si.Sale.SaleDate <= toDate.Value)
                .ToListAsync();

            var categoryPerformance = saleItems
                .Where(si => si.Product?.Category != null)
                .GroupBy(si => si.Product!.Category!.Name)
                .Select(g => new
                {
                    category_name = g.Key,
                    sales_count = g.Count(),
                    revenue = g.Sum(x => x.UnitPrice * x.Quantity),
                    profit = g.Sum(x => (x.UnitPrice - (x.Product?.PurchasePrice ?? 0m)) * x.Quantity)
                })
                .ToList();

            return new
            {
                total_categories = categoryPerformance.Count,
                total_sales = totalSales,
                total_revenue = Math.Round(categoryPerformance.Sum(c => c.revenue), 2),
                total_profit = Math.Round(categoryPerformance.Sum(c => c.profit), 2),
                top_categories = categoryPerformance
                    .OrderByDescending(c => c.revenue)
                    .Take(10)
                    .Select(c => new
                    {
                        category_name = c.category_name,
                        sales_count = c.sales_count,
                        revenue = Math.Round(c.revenue, 2),
                        profit = Math.Round(c.profit, 2),
                        profit_margin = Math.Round(c.revenue > 0 ? (c.profit / c.revenue) * 100m : 0m, 2)
                    }).ToList(),
                currency = "EGP"
            };
        }

        private async Task<object> GetInstallmentsSummaryAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var status = args.TryGetValue("status", out var statusVal) ? statusVal?.ToString() : null;

            var query = _context.Installments
                .AsNoTracking()
                .Include(i => i.Customer)
                .Where(i => i.TenantId == tenantId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(i => i.Status == status);
            }

            var installments = await query.ToListAsync();

            var totalAmount = installments.Sum(i => i.TotalAmount);
            var totalPaid = installments.Sum(i => i.TotalPaid);
            var totalRemaining = installments.Sum(i => Math.Max(0, i.TotalAmount - i.TotalPaid));

            return new
            {
                count = installments.Count,
                total_amount = Math.Round(totalAmount, 2),
                total_paid = Math.Round(totalPaid, 2),
                total_remaining = Math.Round(totalRemaining, 2),
                by_status = installments
                    .GroupBy(i => i.Status ?? "غير محدد")
                    .Select(g => new
                    {
                        status = g.Key,
                        count = g.Count(),
                        remaining = Math.Round(g.Sum(i => Math.Max(0, i.TotalAmount - i.TotalPaid)), 2)
                    })
                    .OrderByDescending(g => g.count)
                    .ToList(),
                top_remaining_customers = installments
                    .GroupBy(i => i.Customer != null ? i.Customer.FullName : "عميل غير معروف")
                    .Select(g => new
                    {
                        customer_name = g.Key,
                        remaining = Math.Round(g.Sum(i => Math.Max(0, i.TotalAmount - i.TotalPaid)), 2)
                    })
                    .OrderByDescending(g => g.remaining)
                    .Take(10)
                    .ToList(),
                currency = "EGP"
            };
        }

        private async Task<object> GetPaymentMethodsSummaryAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");

            EnsureDateRangeIsValid(fromDate, toDate);

            var query = _context.StoreAccounts
                .AsNoTracking()
                .Include(s => s.PaymentMethod)
                .Where(s => s.TenantId == tenantId);

            if (fromDate.HasValue) query = query.Where(s => s.TransactionDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(s => s.TransactionDate <= toDate.Value);

            var rows = await query.ToListAsync();

            var summary = rows
                .GroupBy(r => r.PaymentMethod != null ? r.PaymentMethod.Name : "غير محدد")
                .Select(g => new
                {
                    payment_method = g.Key,
                    total_income = Math.Round(g.Where(x => x.TransactionType == Models.TransactionType.Income).Sum(x => x.Amount), 2),
                    total_expense = Math.Round(g.Where(x => x.TransactionType == Models.TransactionType.Expense).Sum(x => x.Amount), 2),
                    net = Math.Round(g.Where(x => x.TransactionType == Models.TransactionType.Income).Sum(x => x.Amount)
                        - g.Where(x => x.TransactionType == Models.TransactionType.Expense).Sum(x => x.Amount), 2),
                    transactions = g.Count()
                })
                .OrderByDescending(x => x.transactions)
                .ToList();

            return new
            {
                payment_methods = summary,
                count = summary.Count,
                currency = "EGP"
            };
        }

        private async Task<object> GetCustomerInfoAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var name = args.TryGetValue("customer_name", out var val) ? val?.ToString()?.Trim() : null;

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("customer_name is required.");

            // 1. محاولة البحث المباشر
            var customers = await _context.Customers
                .AsNoTracking()
                .Include(c => c.Sales)
                .Where(c => c.TenantId == tenantId && c.FullName.Contains(name))
                .OrderBy(c => c.FullName)
                .ToListAsync();

            // 2. إذا لم يجد نتائج، نجرب البحث الموحد (Normalization)
            if (customers.Count == 0)
            {
                var normalizedName = NormalizeArabicText(name);
                var searchTokens = BuildSearchTokens(name);
                customers = await _context.Customers
                    .AsNoTracking()
                    .Include(c => c.Sales)
                    .Where(c => c.TenantId == tenantId)
                    .Where(c => searchTokens.Count == 0 || searchTokens.Any(token => c.FullName.Contains(token)))
                    .OrderBy(c => c.FullName)
                    .Take(NormalizedSearchCandidateLimit)
                    .ToListAsync();
                
                customers = customers
                    .Where(c => NormalizeArabicText(c.FullName).Contains(normalizedName))
                    .OrderBy(c => c.FullName)
                    .ToList();
            }

            if (customers.Count == 0)
                return new { found = false, message = $"لم يتم العثور على عميل باسم قريب من '{name}'." };

            var customer = customers.First();
            var totalPurchases = customer.Sales.Sum(s => s.TotalAmount);
            var totalPaid = customer.Sales.Sum(s => s.PaidAmount);
            var totalDebt = totalPurchases - totalPaid;

            var recentSales = customer.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(5)
                .Select(s => new
                {
                    date = s.SaleDate.ToString("yyyy-MM-dd"),
                    amount = Math.Round(s.TotalAmount, 2),
                    paid = Math.Round(s.PaidAmount, 2),
                    remaining = Math.Round(s.TotalAmount - s.PaidAmount, 2)
                })
                .ToList();

            return new
            {
                found = true,
                customer_name = customer.FullName,
                phone = customer.PhoneNumber,
                address = customer.Address,
                total_purchases = Math.Round(totalPurchases, 2),
                total_paid = Math.Round(totalPaid, 2),
                total_debt = Math.Round(totalDebt, 2),
                purchases_count = customer.Sales.Count,
                recent_sales = recentSales,
                similar_customers = customers.Count > 1
                    ? customers.Skip(1).Select(c => c.FullName).ToList()
                    : null,
                currency = "EGP"
            };
        }

        private async Task<object> SearchProductAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var name = args.TryGetValue("product_name", out var val) ? val?.ToString()?.Trim() : null;

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("product_name is required.");

            // 1. محاولة البحث المباشر
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.TenantId == tenantId && p.Name.Contains(name))
                .OrderBy(p => p.Name)
                .ToListAsync();

            // 2. إذا لم يجد نتائج، نجرب البحث الموحد (Normalization)
            if (products.Count == 0)
            {
                var normalizedName = NormalizeArabicText(name);
                var searchTokens = BuildSearchTokens(name);
                products = await _context.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.TenantId == tenantId)
                    .Where(p => searchTokens.Count == 0 || searchTokens.Any(token => p.Name.Contains(token)))
                    .OrderBy(p => p.Name)
                    .Take(NormalizedSearchCandidateLimit)
                    .ToListAsync();

                products = products
                    .Where(p => NormalizeArabicText(p.Name).Contains(normalizedName))
                    .OrderBy(p => p.Name)
                    .ToList();
            }

            if (products.Count == 0)
                return new { found = false, message = $"لم يتم العثور على منتج باسم قريب من '{name}'." };

            return new
            {
                found = true,
                count = products.Count,
                products = products.Select(p => new
                {
                    name = p.Name,
                    sale_price = Math.Round(p.SalePrice, 2),
                    purchase_price = Math.Round(p.PurchasePrice, 2),
                    quantity = p.Quantity,
                    category = p.Category != null ? p.Category.Name : "بدون فئة",
                    barcode = p.Barcode,
                    description = p.Description,
                    low_stock = p.Quantity <= 5
                }).ToList(),
                currency = "EGP"
            };
        }

        private async Task<object> GetExpenseDetailsAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");

            EnsureDateRangeIsValid(fromDate, toDate);

            var query = _context.StoreAccounts
                .AsNoTracking()
                .Include(sa => sa.PaymentMethod)
                .Where(sa => sa.TenantId == tenantId && sa.TransactionType == Models.TransactionType.Expense);

            if (fromDate.HasValue) query = query.Where(sa => sa.TransactionDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(sa => sa.TransactionDate <= toDate.Value);

            var expenses = await query
                .OrderByDescending(sa => sa.TransactionDate)
                .Take(50)
                .ToListAsync();

            return new
            {
                count = expenses.Count,
                total_amount = Math.Round(expenses.Sum(e => e.Amount), 2),
                expenses = expenses.Select(e => new
                {
                    date = e.TransactionDate.ToString("yyyy-MM-dd"),
                    name = e.TransactionName,
                    description = e.Description,
                    category = e.Category,
                    amount = Math.Round(e.Amount, 2),
                    payment_method = e.PaymentMethod != null ? e.PaymentMethod.Name : "غير محدد",
                    notes = e.Notes
                }).ToList(),
                currency = "EGP"
            };
        }

        private async Task<object> GetCustomerAccountStatementAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var customerName = args.TryGetValue("customer_name", out var nameVal) ? nameVal?.ToString()?.Trim() : null;
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");
            var includeEntries = ParseNullableBool(args, "include_entries") ?? true;
            var maxEntries = Clamp(ParseInt(args, "max_entries", 30), 1, 200);

            EnsureDateRangeIsValid(fromDate, toDate);

            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("customer_name is required.");

            var normalizedName = NormalizeArabicText(customerName);
            var searchTokens = BuildSearchTokens(customerName);

            var customerCandidates = await _context.Customers
                .AsNoTracking()
                .Where(c => c.TenantId == tenantId)
                .Where(c => c.FullName.Contains(customerName) || searchTokens.Any(token => c.FullName.Contains(token)))
                .OrderBy(c => c.FullName)
                .Take(NormalizedSearchCandidateLimit)
                .ToListAsync();

            var selectedCustomer = customerCandidates
                .OrderBy(c => NormalizeArabicText(c.FullName) == normalizedName ? 0 : 1)
                .ThenBy(c => c.FullName.Length)
                .FirstOrDefault();

            if (selectedCustomer == null)
            {
                return new { found = false, message = $"No customer found close to '{customerName}'." };
            }

            var salesQuery = _context.Sales
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId && s.CustomerId == selectedCustomer.Id);
            if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);

            var paymentsQuery = _context.CustomerPayments
                .AsNoTracking()
                .Include(p => p.PaymentMethod)
                .Where(p => p.TenantId == tenantId && p.CustomerId == selectedCustomer.Id);
            if (fromDate.HasValue) paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= fromDate.Value);
            if (toDate.HasValue) paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= toDate.Value);

            var sales = await salesQuery
                .OrderByDescending(s => s.SaleDate)
                .Take(1000)
                .ToListAsync();
            var payments = await paymentsQuery
                .OrderByDescending(p => p.PaymentDate)
                .Take(1000)
                .ToListAsync();

            var totalSales = sales.Sum(s => s.TotalAmount);
            var totalReturns = sales.Sum(s => s.ReturnedAmount);
            var totalPaidOnSales = sales.Sum(s => s.PaidAmount);
            var totalPayments = payments.Sum(p => p.Amount);
            var netSales = Math.Max(0m, totalSales - totalReturns);
            var remaining = Math.Max(0m, netSales - totalPaidOnSales);
            var unpaidInvoices = sales.Count(s => (s.TotalAmount - s.ReturnedAmount - s.PaidAmount) > 0);

            object? entries = null;
            if (includeEntries)
            {
                var merged = sales.Select(s => new
                {
                    date = s.SaleDate,
                    type = "sale",
                    reference = $"SALE-{s.Id}",
                    debit = Math.Round(Math.Max(0m, s.TotalAmount - s.ReturnedAmount), 2),
                    credit = 0m,
                    note = $"paid={Math.Round(s.PaidAmount, 2)}"
                })
                .Concat(payments.Select(p => new
                {
                    date = p.PaymentDate,
                    type = "payment",
                    reference = $"CPAY-{p.Id}",
                    debit = 0m,
                    credit = Math.Round(p.Amount, 2),
                    note = p.PaymentMethod != null ? p.PaymentMethod.Name : "unknown"
                }))
                .OrderByDescending(x => x.date)
                .Take(maxEntries)
                .ToList();

                entries = merged;
            }

            return new
            {
                found = true,
                customer_id = selectedCustomer.Id,
                customer_name = selectedCustomer.FullName,
                phone = selectedCustomer.PhoneNumber,
                address = selectedCustomer.Address,
                period = new
                {
                    from_date = fromDate?.ToString("yyyy-MM-dd"),
                    to_date = toDate?.ToString("yyyy-MM-dd")
                },
                totals = new
                {
                    invoices_count = sales.Count,
                    unpaid_invoices = unpaidInvoices,
                    total_sales = Math.Round(totalSales, 2),
                    total_returns = Math.Round(totalReturns, 2),
                    net_sales = Math.Round(netSales, 2),
                    paid_on_invoices = Math.Round(totalPaidOnSales, 2),
                    customer_payments = Math.Round(totalPayments, 2),
                    remaining_balance = Math.Round(remaining, 2)
                },
                entries,
                currency = "EGP"
            };
        }

        private async Task<object> GetStoreTransactionsAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");
            var type = args.TryGetValue("transaction_type", out var typeVal) ? typeVal?.ToString()?.Trim() : null;
            var category = args.TryGetValue("category", out var catVal) ? catVal?.ToString()?.Trim() : null;
            var paymentMethodName = args.TryGetValue("payment_method_name", out var pmVal) ? pmVal?.ToString()?.Trim() : null;
            var minAmount = ParseNullableDecimal(args, "min_amount");
            var maxAmount = ParseNullableDecimal(args, "max_amount");
            var limit = Clamp(ParseInt(args, "limit", 50), 1, 300);

            EnsureDateRangeIsValid(fromDate, toDate);
            if (minAmount.HasValue && maxAmount.HasValue && minAmount > maxAmount)
                throw new ArgumentException("min_amount must be less than or equal to max_amount.");

            var query = _context.StoreAccounts
                .AsNoTracking()
                .Include(sa => sa.PaymentMethod)
                .Where(sa => sa.TenantId == tenantId);

            if (fromDate.HasValue) query = query.Where(sa => sa.TransactionDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(sa => sa.TransactionDate <= toDate.Value);
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(sa => sa.Category != null && sa.Category.Contains(category));
            if (minAmount.HasValue) query = query.Where(sa => sa.Amount >= minAmount.Value);
            if (maxAmount.HasValue) query = query.Where(sa => sa.Amount <= maxAmount.Value);
            if (!string.IsNullOrWhiteSpace(paymentMethodName)) query = query.Where(sa => sa.PaymentMethod != null && sa.PaymentMethod.Name.Contains(paymentMethodName));

            if (!string.IsNullOrWhiteSpace(type))
            {
                if (type.Equals("income", StringComparison.OrdinalIgnoreCase) || type.Equals("ايراد", StringComparison.OrdinalIgnoreCase) || type.Equals("إيراد", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(sa => sa.TransactionType == Models.TransactionType.Income);
                else if (type.Equals("expense", StringComparison.OrdinalIgnoreCase) || type.Equals("مصروف", StringComparison.OrdinalIgnoreCase) || type.Equals("مصاريف", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(sa => sa.TransactionType == Models.TransactionType.Expense);
                else
                    throw new ArgumentException("transaction_type must be income/expense or إيراد/مصروف.");
            }

            var rows = await query
                .OrderByDescending(sa => sa.TransactionDate)
                .Take(limit)
                .ToListAsync();

            var income = rows.Where(x => x.TransactionType == Models.TransactionType.Income).Sum(x => x.Amount);
            var expenses = rows.Where(x => x.TransactionType == Models.TransactionType.Expense).Sum(x => x.Amount);

            return new
            {
                count = rows.Count,
                totals = new
                {
                    income = Math.Round(income, 2),
                    expenses = Math.Round(expenses, 2),
                    net = Math.Round(income - expenses, 2)
                },
                transactions = rows.Select(r => new
                {
                    id = r.Id,
                    date = r.TransactionDate.ToString("yyyy-MM-dd"),
                    name = r.TransactionName,
                    type = r.TransactionType.ToString(),
                    amount = Math.Round(r.Amount, 2),
                    category = r.Category,
                    payment_method = r.PaymentMethod != null ? r.PaymentMethod.Name : "unknown",
                    reference = r.ReferenceNumber,
                    notes = r.Notes
                }).ToList(),
                currency = "EGP"
            };
        }

        private async Task<object> GetSalesDetailsAsync(IDictionary<string, object> args)
        {
            var tenantId = await RequireTenantIdAsync();
            var fromDate = ParseDate(args, "from_date");
            var toDate = ParseDate(args, "to_date");
            var customerName = args.TryGetValue("customer_name", out var cVal) ? cVal?.ToString()?.Trim() : null;
            var paymentType = args.TryGetValue("payment_type", out var ptVal) ? ptVal?.ToString()?.Trim() : null;
            var limit = Clamp(ParseInt(args, "limit", 30), 1, 200);

            EnsureDateRangeIsValid(fromDate, toDate);

            var query = _context.Sales
                .AsNoTracking()
                .Include(s => s.Customer)
                .Where(s => s.TenantId == tenantId);

            if (fromDate.HasValue) query = query.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(s => s.SaleDate <= toDate.Value);
            if (!string.IsNullOrWhiteSpace(customerName)) query = query.Where(s => s.Customer != null && s.Customer.FullName.Contains(customerName));

            if (!string.IsNullOrWhiteSpace(paymentType))
            {
                Models.SalePaymentType parsed;
                if (paymentType.Equals("نقدي", StringComparison.OrdinalIgnoreCase))
                    parsed = Models.SalePaymentType.Cash;
                else if (paymentType.Equals("جزئي", StringComparison.OrdinalIgnoreCase))
                    parsed = Models.SalePaymentType.Partial;
                else if (paymentType.Equals("آجل", StringComparison.OrdinalIgnoreCase) || paymentType.Equals("اجل", StringComparison.OrdinalIgnoreCase))
                    parsed = Models.SalePaymentType.Credit;
                else if (!Enum.TryParse<Models.SalePaymentType>(paymentType, true, out parsed))
                    throw new ArgumentException("Invalid payment_type. Allowed: Cash/Partial/Credit or نقدي/جزئي/آجل.");
                query = query.Where(s => s.PaymentType == parsed);
            }

            var sales = await query
                .OrderByDescending(s => s.SaleDate)
                .Take(limit)
                .ToListAsync();

            var totalSales = sales.Sum(s => s.TotalAmount);
            var totalPaid = sales.Sum(s => s.PaidAmount);
            var totalReturned = sales.Sum(s => s.ReturnedAmount);
            var remaining = sales.Sum(s => Math.Max(0m, s.TotalAmount - s.ReturnedAmount - s.PaidAmount));

            return new
            {
                count = sales.Count,
                totals = new
                {
                    gross_sales = Math.Round(totalSales, 2),
                    paid = Math.Round(totalPaid, 2),
                    returns = Math.Round(totalReturned, 2),
                    remaining = Math.Round(remaining, 2)
                },
                invoices = sales.Select(s => new
                {
                    sale_id = s.Id,
                    date = s.SaleDate.ToString("yyyy-MM-dd"),
                    customer = s.Customer != null ? s.Customer.FullName : "walk-in",
                    total = Math.Round(s.TotalAmount, 2),
                    paid = Math.Round(s.PaidAmount, 2),
                    returned = Math.Round(s.ReturnedAmount, 2),
                    remaining = Math.Round(Math.Max(0m, s.TotalAmount - s.ReturnedAmount - s.PaidAmount), 2),
                    payment_type = s.PaymentType.ToString()
                }).ToList(),
                currency = "EGP"
            };
        }

        private async Task<string> RequireTenantIdAsync()
        {
            if (_cachedTenantId != null) return _cachedTenantId;

            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant context is missing for tool execution.");
            }

            _cachedTenantId = tenantId;
            return _cachedTenantId;
        }

        private static void EnsureDateRangeIsValid(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                throw new ArgumentException("Invalid date range: from_date must be before or equal to to_date.");
            }
        }

        private static DateTime? ParseDate(IDictionary<string, object> args, string key)
        {
            if (!args.TryGetValue(key, out var val) || val == null)
            {
                return null;
            }

            return DateTime.TryParse(val.ToString(), out var dt) ? dt : throw new ArgumentException($"Invalid date for '{key}'.");
        }

        private static int ParseInt(IDictionary<string, object> args, string key, int defaultValue)
        {
            if (!args.TryGetValue(key, out var val) || val == null)
            {
                return defaultValue;
            }

            return int.TryParse(val.ToString(), out var result) ? result : throw new ArgumentException($"Invalid integer for '{key}'.");
        }

        private static int? ParseNullableInt(IDictionary<string, object> args, string key)
        {
            if (!args.TryGetValue(key, out var val) || val == null || string.IsNullOrWhiteSpace(val.ToString()))
            {
                return null;
            }

            return int.TryParse(val.ToString(), out var result) ? result : throw new ArgumentException($"Invalid integer for '{key}'.");
        }

        private static bool? ParseNullableBool(IDictionary<string, object> args, string key)
        {
            if (!args.TryGetValue(key, out var val) || val == null || string.IsNullOrWhiteSpace(val.ToString()))
            {
                return null;
            }

            return bool.TryParse(val.ToString(), out var result) ? result : throw new ArgumentException($"Invalid boolean for '{key}'.");
        }

        private static decimal? ParseNullableDecimal(IDictionary<string, object> args, string key)
        {
            if (!args.TryGetValue(key, out var val) || val == null || string.IsNullOrWhiteSpace(val.ToString()))
            {
                return null;
            }

            return decimal.TryParse(val.ToString(), out var result) ? result : throw new ArgumentException($"Invalid decimal for '{key}'.");
        }

        private static int Clamp(int value, int min, int max) => Math.Min(max, Math.Max(min, value));

        private static int ParsePositiveInt(string? value, int fallback)
        {
            if (int.TryParse(value, out var parsed) && parsed > 0)
            {
                return parsed;
            }

            return fallback;
        }

        private static int ParseNonNegativeInt(string? value, int fallback)
        {
            if (int.TryParse(value, out var parsed) && parsed >= 0)
            {
                return parsed;
            }

            return fallback;
        }

        private static (DateTime? start, DateTime? end) GetPeriodRange(string period)
        {
            var now = DateTime.Now;
            return period switch
            {
                "today" => (now.Date, now),
                "week" => (now.Date.AddDays(-(int)now.DayOfWeek), now),
                "month" => (new DateTime(now.Year, now.Month, 1), now),
                "year" => (new DateTime(now.Year, 1, 1), now),
                _ => (null, null)
            };
        }

        private static object SuccessEnvelope(string functionName, object data, int attempt, DateTime startedAtUtc)
        {
            return new
            {
                success = true,
                function = functionName,
                data,
                error = (string?)null,
                meta = new
                {
                    attempt,
                    started_at_utc = startedAtUtc,
                    finished_at_utc = DateTime.UtcNow
                }
            };
        }

        private static object ErrorEnvelope(string code, string message, string functionName, int attempt)
        {
            return new
            {
                success = false,
                function = functionName,
                data = (object?)null,
                error = new
                {
                    code,
                    message
                },
                meta = new
                {
                    attempt,
                    finished_at_utc = DateTime.UtcNow
                }
            };
        }
        private static string NormalizeArabicText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            return text
                .Replace("أ", "ا")
                .Replace("إ", "ا")
                .Replace("آ", "ا")
                .Replace("ة", "ه")
                .Replace("ى", "ي")
                .ToLower()
                .Trim();
        }

        private static List<string> BuildSearchTokens(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            return text
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(token => token.Length >= 2)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(4)
                .ToList();


        }

    }
}
