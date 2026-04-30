using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.Services.SystemSettings;

namespace ManageMentSystem.Services.HomeServices
{
    public class HomeService : IHomeService
    {
        private readonly AppDbContext _context;
        private readonly ISystemSettingsService _systemSettingsService;
        private readonly IUserService _userService;

        public HomeService(AppDbContext context, ISystemSettingsService systemSettingsService, IUserService userService)
        {
            _context = context;
            _systemSettingsService = systemSettingsService;
            _userService = userService;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var userid = await _userService.GetRootUserIdAsync();

            var today = DateTime.Today;
            var startOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfCurrentMonth.AddMonths(-1);
            var endOfLastMonth = startOfCurrentMonth.AddTicks(-1);

            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var totalIncome = await _context.StoreAccounts
                .Where(sa => sa.TransactionType == TransactionType.Income && sa.TenantId == tenantId)
                .SumAsync(sa => sa.Amount);

            var totalExpenses = await _context.StoreAccounts
                .Where(sa => sa.TransactionType == TransactionType.Expense && sa.TenantId == tenantId)
                .SumAsync(sa => sa.Amount);

            var cashBalance = totalIncome - totalExpenses;

            var currentMonthExpenses = await _context.StoreAccounts
                .Where(s => s.TenantId == tenantId && s.TransactionType == TransactionType.Expense && s.TransactionDate >= startOfCurrentMonth)
                .SumAsync(s => s.Amount);

            var lastMonthExpenses = await _context.StoreAccounts
                .Where(s => s.TenantId == tenantId && s.TransactionType == TransactionType.Expense && s.TransactionDate >= startOfLastMonth && s.TransactionDate <= endOfLastMonth)
                .SumAsync(s => s.Amount);

            double expensesTrend = CalculateTrend(currentMonthExpenses, lastMonthExpenses);
            bool isExpensesTrendUp = expensesTrend >= 0;

            var currentMonthRevenue = await  _context.StoreAccounts
                .Where(s => s.TenantId == tenantId && s.TransactionType == TransactionType.Income && s.TransactionDate >= startOfCurrentMonth)
                .SumAsync(s => s.Amount);

            var lastMonthRevenue = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.SaleDate >= startOfLastMonth && s.SaleDate <= endOfLastMonth)
                .SumAsync(s => s.TotalAmount);

            double revenueTrend = CalculateTrend(currentMonthRevenue, lastMonthRevenue);
            bool isRevenueTrendUp = revenueTrend >= 0;

            var currentMonthSalesCount = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.SaleDate >= startOfCurrentMonth)
                .CountAsync();

            var lastMonthSalesCount = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.SaleDate >= startOfLastMonth && s.SaleDate <= endOfLastMonth)
                .CountAsync();

            double salesCountTrend = 0;
            if (lastMonthSalesCount > 0)
            {
                salesCountTrend = (double)(((decimal)currentMonthSalesCount - lastMonthSalesCount) / lastMonthSalesCount * 100);
            }
            else if (currentMonthSalesCount > 0) salesCountTrend = 100;

            bool isSalesCountTrendUp = salesCountTrend >= 0;

            var currentMonthProfit = await GetTotalProfitAsync(tenantId, startOfCurrentMonth, DateTime.MaxValue);
            var lastMonthProfit = await GetTotalProfitAsync(tenantId, startOfLastMonth, endOfLastMonth);

            double profitTrend = CalculateTrend(currentMonthProfit, lastMonthProfit);
            bool isProfitTrendUp = profitTrend >= 0;

            var activeCustomersCount = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.SaleDate >= startOfCurrentMonth)
                .Select(s => s.CustomerId)
                .Distinct()
                .CountAsync();

            var stockTotalValue = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .SumAsync(p => p.Quantity * p.PurchasePrice);

            var inventorySettings = await _systemSettingsService.GetInventorySettingsAsync();

            return new DashboardViewModel
            {
                TotalProducts = await _context.Products.Where(p => p.TenantId == tenantId).Select(p => p.Quantity).SumAsync(),
                TotalCustomers = await _context.Customers.Where(c => c.TenantId == tenantId).CountAsync(),
                TotalSales = await _context.Sales.Where(c => c.TenantId == tenantId).CountAsync(),
                TotalRevenue = await _context.Sales.Where(c => c.TenantId == tenantId).SumAsync(s => s.TotalAmount),
                CashBalance = cashBalance,

                MonthlyRevenue = currentMonthRevenue,
                RevenueTrend = Math.Round(Math.Abs(revenueTrend), 1),
                IsRevenueTrendUp = isRevenueTrendUp,

                MonthlySalesCount = currentMonthSalesCount,
                SalesCountTrend = Math.Round(Math.Abs(salesCountTrend), 1),
                IsSalesCountTrendUp = isSalesCountTrendUp,
                MonthlyExpenses = currentMonthExpenses,
                ExpensesTrend = Math.Round(Math.Abs(expensesTrend), 1),
                IsExpensesTrendUp = isExpensesTrendUp,

                MonthlyProfit = currentMonthProfit,
                ProfitTrend = Math.Round(Math.Abs(profitTrend), 1),
                IsProfitTrendUp = isProfitTrendUp,

                ActiveCustomersCount = activeCustomersCount,
                StockTotalValue = stockTotalValue,

                RecentSales = await _context.Sales.Where(c => c.TenantId == tenantId)
                    .Include(s => s.Customer)
                    .OrderByDescending(s => s.SaleDate)
                    .Take(5)
                    .ToListAsync(),

                LowStockProducts = await _context.Products
                    .Where(p => p.Quantity <= inventorySettings.LowStockThreshold && p.TenantId == tenantId)
                    .Take(5)
                    .ToListAsync()
            };
        }

        private double CalculateTrend(decimal current, decimal last)
        {
            if (last > 0)
                return (double)((current - last) / last * 100);
            else if (current > 0)
                return 100;
            return 0;
        }

        private async Task<decimal> GetTotalProfitAsync(string tenantId, DateTime startDate, DateTime endDate)
        {
            var salesProfit = await GetSalesProfitAsync(tenantId, startDate, endDate);
            var installmentsProfit = await GetInstallmentsProfitAsync(tenantId, startDate, endDate);
            return salesProfit + installmentsProfit;
        }

        private async Task<decimal> GetSalesProfitAsync(string tenantId, DateTime startDate, DateTime endDate)
        {
            var customerPaymentsQuery = _context.CustomerPayments
                .Where(cp => cp.Customer.TenantId == tenantId && cp.PaymentDate >= startDate && cp.PaymentDate <= endDate);

            var customerPayments = await customerPaymentsQuery
                .Include(cp => cp.Allocations)
                .ThenInclude(a => a.Sale)
                .ThenInclude(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .ToListAsync();

            var totalRealizedProfit = 0m;
            foreach (var customerPayment in customerPayments)
            {
                var realizedProfitFromPayment = 0m;
                var paymentSign = customerPayment.Amount < 0 ? -1m : 1m;

                foreach (var allocation in customerPayment.Allocations)
                {
                    var sale = allocation.Sale;
                    if (sale != null && sale.SaleItems != null && sale.SaleItems.Any())
                    {
                        var totalCostOfGoods = sale.SaleItems
                            .Where(si => si.Product != null)
                            .Sum(si => si.Product.PurchasePrice * si.Quantity);

                        var grossAmountAfterDiscount = sale.TotalAmount;
                        var netSaleAmount = Math.Max(0, grossAmountAfterDiscount - sale.ReturnedAmount);

                        decimal adjustedCostOfGoods = totalCostOfGoods;
                        if (grossAmountAfterDiscount > 0 && netSaleAmount < grossAmountAfterDiscount)
                        {
                            var keptRatio = netSaleAmount / grossAmountAfterDiscount;
                            adjustedCostOfGoods = totalCostOfGoods * keptRatio;
                        }

                        var profitMargin = netSaleAmount > 0 ? (netSaleAmount - adjustedCostOfGoods) / netSaleAmount : 0;
                        var profitFromAllocation = allocation.Amount * profitMargin * paymentSign;
                        realizedProfitFromPayment += profitFromAllocation;
                    }
                }
                totalRealizedProfit += realizedProfitFromPayment;
            }
            return totalRealizedProfit;
        }

        private async Task<decimal> GetInstallmentsProfitAsync(string tenantId, DateTime startDate, DateTime endDate)
        {
            var downPayments = await _context.StoreAccounts
                .Where(sa => sa.Category == "الأقساط" &&
                             sa.ReferenceNumber.StartsWith("INST-DOWN") &&
                             sa.TenantId == tenantId &&
                             sa.TransactionDate >= startDate && sa.TransactionDate <= endDate)
                .ToListAsync();

            var installmentPayments = await _context.StoreAccounts
                .Where(sa => sa.Category == "الأقساط" &&
                             sa.ReferenceNumber.StartsWith("INST-PAYMENT") &&
                             sa.TenantId == tenantId &&
                             sa.TransactionDate >= startDate && sa.TransactionDate <= endDate)
                .ToListAsync();

            var totalProfit = 0m;

            var installmentIds = downPayments
                .Select(dp => dp.ReferenceNumber.Replace("INST-DOWN-", "").Trim())
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();

            if (installmentIds.Any())
            {
                var installmentsWithProducts = await _context.Installments
                    .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                    .Where(i => installmentIds.Contains(i.Id) && i.TenantId == tenantId)
                    .ToListAsync();

                foreach (var downPayment in downPayments)
                {
                    if (int.TryParse(downPayment.ReferenceNumber.Replace("INST-DOWN-", "").Trim(), out int instId))
                    {
                        var installment = installmentsWithProducts.FirstOrDefault(i => i.Id == instId);
                        if (installment?.InstallmentItems?.Any() == true)
                        {
                            var totalPurchasePrice = installment.InstallmentItems.Sum(ii => (ii.Product?.PurchasePrice ?? 0) * ii.Quantity);
                            var totalAmountWithInterest = installment.TotalWithInterest + installment.ExtraMonthAmount;
                            var installmentTotalProfit = totalAmountWithInterest - totalPurchasePrice;
                            var profitRatio = totalAmountWithInterest > 0 ? installmentTotalProfit / totalAmountWithInterest : 0;
                            totalProfit += downPayment.Amount * profitRatio;
                        }
                    }
                }
            }

            var paymentIds = installmentPayments
                .Select(ip => ip.ReferenceNumber.Replace("INST-PAYMENT-", "").Trim())
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();

            if (paymentIds.Any())
            {
                var paymentsWithDetails = await _context.InstallmentPayments
                    .Include(ip => ip.Installment)
                    .ThenInclude(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                    .Where(ip => paymentIds.Contains(ip.Id) && ip.Installment.TenantId == tenantId)
                    .ToListAsync();

                foreach (var payment in installmentPayments)
                {
                    if (int.TryParse(payment.ReferenceNumber.Replace("INST-PAYMENT-", "").Trim(), out int payId))
                    {
                        var paymentDetail = paymentsWithDetails.FirstOrDefault(p => p.Id == payId);
                        if (paymentDetail?.Installment?.InstallmentItems?.Any() == true)
                        {
                            var totalPurchasePrice = paymentDetail.Installment.InstallmentItems.Sum(ii => (ii.Product?.PurchasePrice ?? 0) * ii.Quantity);
                            var totalAmountWithInterest = paymentDetail.Installment.TotalWithInterest + paymentDetail.Installment.ExtraMonthAmount;
                            var paymentTotalProfit = totalAmountWithInterest - totalPurchasePrice;
                            var profitRatio = totalAmountWithInterest > 0 ? paymentTotalProfit / totalAmountWithInterest : 0;
                            totalProfit += payment.Amount * profitRatio;
                        }
                    }
                }
            }

            return totalProfit;
        }


    }
}
