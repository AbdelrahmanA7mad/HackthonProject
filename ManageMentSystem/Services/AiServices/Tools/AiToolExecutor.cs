using ManageMentSystem.Data;
using ManageMentSystem.Services.UserServices;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ManageMentSystem.Services.AiServices
{
    public class AiToolExecutor : IAiToolExecutor
    {
        private static readonly HashSet<string> AllowedPeriods = new(StringComparer.OrdinalIgnoreCase)
        {
            "today", "week", "month", "year", "all"
        };

        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IAiTelemetryService _telemetry;

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
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Where(s => s.TenantId == tenantId);

            if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);

            var sales = await salesQuery.ToListAsync();
            var salesRevenue = sales.Sum(s => s.TotalAmount);
            var costOfGoods = sales.SelectMany(s => s.SaleItems).Sum(si => (si.Product?.PurchasePrice ?? 0m) * si.Quantity);
            var grossProfit = salesRevenue - costOfGoods;

            var expenseQuery = _context.StoreAccounts
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

            var query = _context.StoreAccounts.Where(sa => sa.TenantId == tenantId);
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

            var salesQuery = _context.Sales.Where(s => s.TenantId == tenantId);
            if (periodStart.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= periodStart.Value);
            if (periodEnd.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= periodEnd.Value);

            var sales = await salesQuery
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ToListAsync();

            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var costOfGoods = sales.SelectMany(s => s.SaleItems).Sum(si => (si.Product?.PurchasePrice ?? 0m) * si.Quantity);
            var expensesQuery = _context.StoreAccounts
                .Where(sa => sa.TenantId == tenantId && sa.TransactionType == Models.TransactionType.Expense);
            if (periodStart.HasValue) expensesQuery = expensesQuery.Where(sa => sa.TransactionDate >= periodStart.Value);
            if (periodEnd.HasValue) expensesQuery = expensesQuery.Where(sa => sa.TransactionDate <= periodEnd.Value);
            var expenses = await expensesQuery.SumAsync(sa => (decimal?)sa.Amount) ?? 0m;

            var totalCustomers = await _context.Customers.CountAsync(c => c.TenantId == tenantId);
            var totalInventory = await _context.Products.Where(p => p.TenantId == tenantId).SumAsync(p => (int?)p.Quantity) ?? 0;
            var lowStockCount = await _context.Products.CountAsync(p => p.TenantId == tenantId && p.Quantity <= 5);

            return new
            {
                total_sales_count = sales.Count,
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
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Where(s => s.TenantId == tenantId);

            if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);
            if (customerId.HasValue) salesQuery = salesQuery.Where(s => s.CustomerId == customerId.Value);
            if (categoryId.HasValue) salesQuery = salesQuery.Where(s => s.SaleItems.Any(si => si.Product != null && si.Product.CategoryId == categoryId.Value));

            var sales = await salesQuery.ToListAsync();
            var totalRevenue = sales.Sum(s => s.TotalAmount);
            var totalDiscount = sales.Sum(s => s.DiscountAmount);
            var netRevenue = totalRevenue - totalDiscount;
            var totalCost = sales.SelectMany(s => s.SaleItems).Sum(si => (si.Product?.PurchasePrice ?? 0m) * si.Quantity);
            var grossProfit = netRevenue - totalCost;
            var profitMargin = netRevenue > 0 ? (grossProfit / netRevenue) * 100m : 0m;
            var dailySales = sales
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new { date = g.Key, count = g.Count(), revenue = g.Sum(x => x.TotalAmount) })
                .OrderByDescending(x => x.revenue)
                .Take(5)
                .ToList();

            return new
            {
                total_sales = sales.Count,
                total_revenue = Math.Round(totalRevenue, 2),
                total_discount = Math.Round(totalDiscount, 2),
                net_revenue = Math.Round(netRevenue, 2),
                total_cost = Math.Round(totalCost, 2),
                gross_profit = Math.Round(grossProfit, 2),
                profit_margin = Math.Round(profitMargin, 2),
                average_sale_value = Math.Round(sales.Count > 0 ? totalRevenue / sales.Count : 0m, 2),
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

            var customers = await _context.Customers
                .Include(c => c.Sales)
                .Where(c => c.TenantId == tenantId)
                .ToListAsync();

            var salesQuery = _context.Sales.Where(s => s.TenantId == tenantId);
            if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);
            var sales = await salesQuery.Include(s => s.Customer).ToListAsync();

            var topCustomers = sales
                .Where(s => s.Customer != null)
                .GroupBy(s => s.Customer!.FullName)
                .Select(g => new { customer_name = g.Key, sales_count = g.Count(), total_spent = g.Sum(x => x.TotalAmount) })
                .OrderByDescending(x => x.total_spent)
                .Take(10)
                .ToList();

            return new
            {
                total_customers = customers.Count,
                active_customers = customers.Count(c => c.Sales.Any()),
                new_customers = customers.Count(c => (!fromDate.HasValue || c.CreatedAt >= fromDate.Value) && (!toDate.HasValue || c.CreatedAt <= toDate.Value)),
                total_customer_revenue = Math.Round(sales.Sum(s => s.TotalAmount), 2),
                average_customer_value = Math.Round(customers.Count > 0 ? sales.Sum(s => s.TotalAmount) / customers.Count : 0m, 2),
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

            var transactionsQuery = _context.StoreAccounts.Where(sa => sa.TenantId == tenantId);
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

            var query = _context.GeneralDebts.Where(gd => gd.TenantId == tenantId);
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

            var salesQuery = _context.Sales
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ThenInclude(p => p.Category)
                .Where(s => s.TenantId == tenantId);

            if (fromDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue) salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);

            var sales = await salesQuery.ToListAsync();
            var categoryPerformance = sales
                .SelectMany(s => s.SaleItems)
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
                total_sales = sales.Count,
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

        private async Task<string> RequireTenantIdAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant context is missing for tool execution.");
            }

            return tenantId;
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
    }
}
