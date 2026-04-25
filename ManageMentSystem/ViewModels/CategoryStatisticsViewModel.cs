using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CategoryStatisticsViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        
        // إحصائيات المنتجات
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalPurchaseValue { get; set; }
        public decimal TotalSaleValue { get; set; }
        public decimal PotentialProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        
        // إحصائيات المبيعات
        public int TotalSalesCount { get; set; }
        public decimal TotalSalesValue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalUnitsSold { get; set; }
        
        // إحصائيات المخزون
        public int LowStockProducts { get; set; } // منتجات بكمية أقل من 5
        public int OutOfStockProducts { get; set; } // منتجات نفذت
        public int HighValueProducts { get; set; } // منتجات عالية القيمة
        
        // المنتجات في الفئة
        public List<ProductStatisticsViewModel> Products { get; set; } = new List<ProductStatisticsViewModel>();
        
        // المبيعات الحديثة
        public List<SaleStatisticsViewModel> RecentSales { get; set; } = new List<SaleStatisticsViewModel>();
        
        // إحصائيات خاصة بالآيفون
        public List<IphoneStatisticsViewModel> IphoneProducts { get; set; } = new List<IphoneStatisticsViewModel>();
    }

    public class ProductStatisticsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public string? Barcode { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalSalesValue { get; set; }
        public decimal Profit { get; set; }
        public string StockStatus { get; set; } // "متوفر", "كمية قليلة", "نفذ"
        public DateTime LastSaleDate { get; set; }
    }

    public class SaleStatisticsViewModel
    {
        public int SaleId { get; set; }
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime SaleDate { get; set; }
    }

    public class IphoneStatisticsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public string? Barcode { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalSalesValue { get; set; }
        public decimal Profit { get; set; }
        public DateTime LastSaleDate { get; set; }
        public string StockStatus { get; set; }
    }
} 