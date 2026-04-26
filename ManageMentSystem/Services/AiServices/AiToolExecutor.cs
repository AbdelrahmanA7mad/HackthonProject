using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.StatisticsServices;
using ManageMentSystem.Services.UserServices;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Services.AiServices
{
    /// <summary>
    /// ينفذ الـ functions التي يطلبها Gemini عبر الـ Services الموجودة
    /// </summary>
    public class AiToolExecutor : IAiToolExecutor
    {
        private readonly AppDbContext _context;
        private readonly IStatisticsService _statistics;
        private readonly IUserService _userService;

        public AiToolExecutor(
            AppDbContext context,
            IStatisticsService statistics,
            IUserService userService)
        {
            _context   = context;
            _statistics = statistics;
            _userService = userService;
        }

        public async Task<object> ExecuteAsync(string functionName, IDictionary<string, object> args)
        {
            try
            {
                return functionName switch
                {
                    "get_total_sales"         => await GetTotalSalesAsync(args),
                    "get_top_products"        => await GetTopProductsAsync(args),
                    "get_monthly_sales"       => await GetMonthlySalesAsync(args),
                    "get_profit"              => await GetProfitAsync(args),
                    "get_low_stock_products"  => await GetLowStockProductsAsync(args),
                    "get_top_customers"       => await GetTopCustomersAsync(args),
                    "get_store_account_summary" => await GetStoreAccountSummaryAsync(args),
                    "get_pending_debts"       => await GetPendingDebtsAsync(),
                    "get_general_statistics"  => await GetGeneralStatisticsAsync(args),
                    _ => new { error = $"Function '{functionName}' غير معروفة" }
                };
            }
            catch (Exception ex)
            {
                return new { error = $"خطأ في تنفيذ {functionName}: {ex.Message}" };
            }
        }

        // ─── المبيعات ─────────────────────────────────────────────────────────

        private async Task<object> GetTotalSalesAsync(IDictionary<string, object> args)
        {
            var tenantId  = await _userService.GetCurrentTenantIdAsync();
            var fromDate  = ParseDate(args, "from_date");
            var toDate    = ParseDate(args, "to_date");

            var query = _context.Sales.Where(s => s.TenantId == tenantId);

            if (fromDate.HasValue) query = query.Where(s => s.SaleDate >= fromDate.Value);
            if (toDate.HasValue)   query = query.Where(s => s.SaleDate <= toDate.Value);

            var totalAmount = await query.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;
            var count       = await query.CountAsync();

            return new
            {
                total_amount  = Math.Round(totalAmount, 2),
                sales_count   = count,
                from_date     = fromDate?.ToString("yyyy-MM-dd") ?? "بداية التاريخ",
                to_date       = toDate?.ToString("yyyy-MM-dd")   ?? "اليوم",
                currency      = "جنيه"
            };
        }

        private async Task<object> GetTopProductsAsync(IDictionary<string, object> args)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var topN     = ParseInt(args, "top_n", 5);
            var fromDate = ParseDate(args, "from_date");
            var toDate   = ParseDate(args, "to_date");

            var query = _context.SaleItems
                .Include(si => si.Product)
                .Include(si => si.Sale)
                .Where(si => si.Sale.TenantId == tenantId && si.Product != null);

            if (fromDate.HasValue) query = query.Where(si => si.Sale.SaleDate >= fromDate.Value);
            if (toDate.HasValue)   query = query.Where(si => si.Sale.SaleDate <= toDate.Value);

            var topProducts = await query
                .GroupBy(si => new { si.ProductId, si.Product.Name, si.Product.SalePrice })
                .Select(g => new
                {
                    product_name   = g.Key.Name,
                    total_quantity = g.Sum(si => si.Quantity),
                    total_revenue  = g.Sum(si => si.UnitPrice * si.Quantity),
                    sale_price     = g.Key.SalePrice
                })
                .OrderByDescending(p => p.total_quantity)
                .Take(topN)
                .ToListAsync();

            return new { top_products = topProducts, count = topProducts.Count };
        }

        private async Task<object> GetMonthlySalesAsync(IDictionary<string, object> args)
        {
            var year   = ParseInt(args, "year", DateTime.Now.Year);
            var result = await _statistics.GetMonthlyRevenueAsync(year);
            return new
            {
                year,
                monthly_data = result.Select(m => new
                {
                    month   = m.Month,
                    revenue = Math.Round(m.Revenue, 2)
                })
            };
        }

        // ─── الأرباح ─────────────────────────────────────────────────────────

        private async Task<object> GetProfitAsync(IDictionary<string, object> args)
        {
            var fromDate = ParseDate(args, "from_date");
            var toDate   = ParseDate(args, "to_date");

            var report = await _statistics.GetProfitLossReportAsync(fromDate, toDate);

            return new
            {
                sales_revenue      = Math.Round(report.SalesRevenue, 2),
                cost_of_goods      = Math.Round(report.CostOfGoodsSold, 2),
                gross_profit       = Math.Round(report.GrossProfit, 2),
                operating_expenses = Math.Round(report.OperatingExpenses, 2),
                net_profit         = Math.Round(report.NetProfit, 2),
                profit_margin_pct  = Math.Round(report.ProfitMargin, 2),
                currency           = "جنيه"
            };
        }

        // ─── المخزون ─────────────────────────────────────────────────────────

        private async Task<object> GetLowStockProductsAsync(IDictionary<string, object> args)
        {
            var tenantId  = await _userService.GetCurrentTenantIdAsync();
            var threshold = ParseInt(args, "threshold", 5);

            var products = await _context.Products
                .Where(p => p.TenantId == tenantId && p.Quantity <= threshold)
                .Include(p => p.Category)
                .OrderBy(p => p.Quantity)
                .Select(p => new
                {
                    name          = p.Name,
                    current_stock = p.Quantity,
                    category      = p.Category != null ? p.Category.Name : "بدون فئة",
                    sale_price    = p.SalePrice
                })
                .ToListAsync();

            return new
            {
                low_stock_products = products,
                count              = products.Count,
                threshold
            };
        }

        // ─── العملاء ─────────────────────────────────────────────────────────

        private async Task<object> GetTopCustomersAsync(IDictionary<string, object> args)
        {
            var topN   = ParseInt(args, "top_n", 5);
            var report = await _statistics.GetCustomerReportAsync(null, null);

            var top = report.TopCustomers
                .Take(topN)
                .Select(c => new
                {
                    name          = c.CustomerName,
                    total_spent   = Math.Round(c.TotalSpent, 2),
                    purchases_count = c.SalesCount
                })
                .ToList();

            return new { top_customers = top, count = top.Count, currency = "جنيه" };
        }

        // ─── الخزينة ─────────────────────────────────────────────────────────

        private async Task<object> GetStoreAccountSummaryAsync(IDictionary<string, object> args)
        {
            var fromDate = ParseDate(args, "from_date");
            var toDate   = ParseDate(args, "to_date");

            var report = await _statistics.GetStoreAccountReportAsync(fromDate, toDate, null);

            return new
            {
                total_income   = Math.Round(report.TotalIncome, 2),
                total_expenses = Math.Round(report.TotalExpenses, 2),
                net_balance    = Math.Round(report.NetBalance, 2),
                transactions_count = report.TotalTransactions,
                currency       = "جنيه"
            };
        }

        private async Task<object> GetPendingDebtsAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();

            // الديون العامة
            var generalDebts = await _context.GeneralDebts
                .Where(gd => gd.TenantId == tenantId)
                .SumAsync(gd => gd.Amount - gd.PaidAmount);

            // الأقساط المتبقية
            var installmentsDebt = await _context.Installments
                .Where(i => i.TenantId == tenantId)
                .SumAsync(i => (decimal?)(i.TotalAmount - i.TotalPaid)) ?? 0;

            // ديون العملاء من المبيعات الآجلة
            var customerDebts = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.PaidAmount < s.TotalAmount)
                .SumAsync(s => (decimal?)(s.TotalAmount - s.PaidAmount)) ?? 0;

            return new
            {
                general_debts         = Math.Round(generalDebts, 2),
                installments_remaining = Math.Round(installmentsDebt, 2),
                customer_debts        = Math.Round(customerDebts, 2),
                total_pending         = Math.Round(generalDebts + installmentsDebt + customerDebts, 2),
                currency              = "جنيه"
            };
        }

        // ─── الإحصائيات العامة ───────────────────────────────────────────────

        private async Task<object> GetGeneralStatisticsAsync(IDictionary<string, object> args)
        {
            var period = args.TryGetValue("period", out var p) ? p?.ToString() ?? "all" : "all";
            var stats  = await _statistics.GetFilteredStatisticsAsync(period, null, null, null);

            return new
            {
                total_sales_count  = stats.TotalSales,
                total_revenue      = Math.Round(stats.TotalRevenue, 2),
                total_customers    = stats.TotalCustomers,
                total_inventory    = stats.TotalProducts,
                net_profit         = Math.Round(stats.NetProfit, 2),
                low_stock_count    = stats.LowStockCount,
                period,
                currency           = "جنيه"
            };
        }

        // ─── Helper Methods ───────────────────────────────────────────────────

        private static DateTime? ParseDate(IDictionary<string, object> args, string key)
        {
            if (args.TryGetValue(key, out var val) && val != null)
            {
                if (DateTime.TryParse(val.ToString(), out var dt))
                    return dt;
            }
            return null;
        }

        private static int ParseInt(IDictionary<string, object> args, string key, int defaultValue)
        {
            if (args.TryGetValue(key, out var val) && val != null)
            {
                if (int.TryParse(val.ToString(), out var result))
                    return result;
            }
            return defaultValue;
        }
    }
}
