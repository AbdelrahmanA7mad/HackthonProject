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

            // 1. ????? ???????? (????? ????? ?????? ?????? ??????)
            var today = DateTime.Today;
            var startOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfCurrentMonth.AddMonths(-1);
            var endOfLastMonth = startOfCurrentMonth.AddDays(-1);

            // =========================================================
            // 2. ???????? ??????? ?????? (?????? ????? - Lifetime)
            // =========================================================
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var totalIncome = await _context.StoreAccounts
                .Where(sa => sa.TransactionType == TransactionType.Income && sa.TenantId == tenantId)
                .SumAsync(sa => sa.Amount);

            var totalExpenses = await _context.StoreAccounts
                .Where(sa => sa.TransactionType == TransactionType.Expense && sa.TenantId == tenantId)
                .SumAsync(sa => sa.Amount);

            var cashBalance = totalIncome - totalExpenses;

            // =========================================================
            // 4. ???????? ??????? (Monthly Expenses) - ?????? ?????
            // =========================================================
            var currentMonthExpenses = await _context.StoreAccounts
                .Where(s => s.TenantId == tenantId && s.TransactionType == TransactionType.Expense && s.TransactionDate >= startOfCurrentMonth)
                .SumAsync(s => s.Amount);

            var lastMonthExpenses = await _context.StoreAccounts
                .Where(s => s.TenantId == tenantId && s.TransactionType == TransactionType.Expense && s.TransactionDate >= startOfLastMonth && s.TransactionDate <= endOfLastMonth)
                .SumAsync(s => s.Amount);

            double expensesTrend = CalculateTrend(currentMonthExpenses, lastMonthExpenses);
            bool isExpensesTrendUp = expensesTrend >= 0;

            // =========================================================
            // 5. ?????? ????? (Sales Revenue & Count) - ?????? ?????
            // =========================================================

            // ?) ????????? (Revenue)
            var currentMonthRevenue = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.SaleDate >= startOfCurrentMonth)
                .SumAsync(s => s.TotalAmount);

            var lastMonthRevenue = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.SaleDate >= startOfLastMonth && s.SaleDate <= endOfLastMonth)
                .SumAsync(s => s.TotalAmount);

            double revenueTrend = CalculateTrend(currentMonthRevenue, lastMonthRevenue);
            bool isRevenueTrendUp = revenueTrend >= 0;

            // ?) ??? ???????? (Count)
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

            // =========================================================
            // 6. ???????? ?????? (??????? ??????? + ???? ???????)
            // =========================================================

            // ??????? ??????? ??? ????? (?? ????? ???????)
            var activeCustomersCount = await _context.Sales
                .Where(s => s.TenantId == tenantId && s.SaleDate >= startOfCurrentMonth)
                .Select(s => s.CustomerId)
                .Distinct()
                .CountAsync();

            // ?????? ????????? ??????? (?????? * ??? ??????)
            // ???? ?? ??? ??????? ?? ??????? ?? PurchasePrice
            var stockTotalValue = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .SumAsync(p => p.Quantity * p.PurchasePrice);

            // =========================================================
            // 7. ????? ???????? ?????? ??? ViewModel
            // =========================================================
            var inventorySettings = await _systemSettingsService.GetInventorySettingsAsync();

            return new DashboardViewModel
            {
                // ???????? ???????? (??????)
                TotalProducts = await _context.Products.Where(p => p.TenantId == tenantId).Select(p => p.Quantity).SumAsync(),
                TotalCustomers = await _context.Customers.Where(c => c.TenantId == tenantId).CountAsync(),
                TotalSales = await _context.Sales.Where(c => c.TenantId == tenantId).CountAsync(),
                TotalRevenue = await _context.Sales.Where(c => c.TenantId == tenantId).SumAsync(s => s.TotalAmount),
                CashBalance = cashBalance,

                // ???????? ??????? ???????????? (???????)
                MonthlyRevenue = currentMonthRevenue,
                RevenueTrend = Math.Round(Math.Abs(revenueTrend), 1),
                IsRevenueTrendUp = isRevenueTrendUp,

                MonthlySalesCount = currentMonthSalesCount,
                SalesCountTrend = Math.Round(Math.Abs(salesCountTrend), 1),
                IsSalesCountTrendUp = isSalesCountTrendUp,
                MonthlyExpenses = currentMonthExpenses,
                ExpensesTrend = Math.Round(Math.Abs(expensesTrend), 1),
                IsExpensesTrendUp = isExpensesTrendUp,

                ActiveCustomersCount = activeCustomersCount,
                StockTotalValue = stockTotalValue,

                // ???????
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

        // ???? ?????? ????? ?????? ???????
        private double CalculateTrend(decimal current, decimal last)
        {
            if (last > 0)
                return (double)((current - last) / last * 100);
            else if (current > 0)
                return 100;
            return 0;
        }


    }
}
