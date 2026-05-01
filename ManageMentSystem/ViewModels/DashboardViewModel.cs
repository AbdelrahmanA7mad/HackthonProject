using ManageMentSystem.Models;
using System.Collections.Generic;

namespace ManageMentSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CashBalance { get; set; }
        public List<Sale> RecentSales { get; set; }
        public List<Product> LowStockProducts { get; set; }

        public decimal TotalReturns { get; set; }
        public double ReturnsTrend { get; set; }
        public bool IsReturnsTrendUp { get; set; }

        public decimal MonthlyExpenses { get; set; }
        public double ExpensesTrend { get; set; }
        public bool IsExpensesTrendUp { get; set; }

        // 1. إيرادات الشهر (Money)
        public decimal MonthlyRevenue { get; set; }
        public double RevenueTrend { get; set; }
        public bool IsRevenueTrendUp { get; set; }

        // 2. عدد فواتير الشهر (Count)
        public int MonthlySalesCount { get; set; }
        public double SalesCountTrend { get; set; }
        public bool IsSalesCountTrendUp { get; set; }

        public int ActiveCustomersCount { get; set; } // نشط هذا الشهر
        public decimal StockTotalValue { get; set; }  // قيمة المخزون

        // 3. الأرباح (Profit)
        public decimal MonthlyProfit { get; set; }
        public double ProfitTrend { get; set; }
        public bool IsProfitTrendUp { get; set; }
    }
} 