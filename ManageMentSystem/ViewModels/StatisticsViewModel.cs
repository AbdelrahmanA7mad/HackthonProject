using ManageMentSystem.Models;
using System.Collections.Generic;

namespace ManageMentSystem.ViewModels
{
    public class StatisticsViewModel
    {
        // Simple Filter Properties
        public string Period { get; set; } = "all"; // all, today, week, month, year, custom
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }

        // Core Statistics
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalAllRevenue { get; set; } // إجمالي الإيرادات (بيع)

        // Period Statistics
        public int PeriodSales { get; set; }
        public decimal PeriodRevenue { get; set; }

        // Recent Data
        public List<Sale> RecentSales { get; set; } = new List<Sale>();

        // Filter Options
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public int LowStockCount { get; set; } // عدد المنتجات الناقصة
        public decimal NetProfit { get; set; } // صافي الربح
    }
} 