using ManageMentSystem.Models;
using ManageMentSystem.Services.StatisticsServices;
using System;
using System.Collections.Generic;
using static ManageMentSystem.Services.StatisticsServices.StatisticsService;

namespace ManageMentSystem.ViewModels
{
    public class ComprehensiveReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Inventory
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public decimal InventoryValue { get; set; }
        public List<Product> Products { get; set; }

        // Sales
        public int TotalSales { get; set; }
        public decimal TotalSalesRevenue { get; set; }
        public decimal NetSalesProfit { get; set; }
        public List<Sale> Sales { get; set; }

        // Customers
        public int TotalCustomers { get; set; }
        public List<Customer> Customers { get; set; }

        // Total Revenue
        public decimal TotalRevenue { get; set; }

        // Chart Data
        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; }
        public List<MonthlySalesViewModel> MonthlySales { get; set; }
    }

    // تقارير جديدة شاملة
    public class SalesReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CustomerId { get; set; }
        public int? CategoryId { get; set; }
        public string? CustomerSearch { get; set; }
        public string? CustomerName { get; set; }
        public string? CategoryName { get; set; }
        
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int TotalItems { get; set; }
        public decimal AverageSaleValue { get; set; }
        
        public List<Sale> Sales { get; set; } = new List<Sale>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<Category> Categories { get; set; } = new List<Category>();
        
        // إحصائيات يومية
        public List<DailySalesViewModel> DailySales { get; set; } = new List<DailySalesViewModel>();
    }

    public class InventoryReportViewModel
    {
        public int? CategoryId { get; set; }
        public bool? LowStockOnly { get; set; }
        public string? CategoryName { get; set; }
        
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public decimal AverageProductValue { get; set; }
        public int TotalCategories { get; set; }
        
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        
        // إحصائيات حسب الفئة
        public List<CategoryInventoryViewModel> CategoryInventory { get; set; } = new List<CategoryInventoryViewModel>();
    }

    public class CustomerReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomers { get; set; }
        public decimal TotalCustomerRevenue { get; set; }
        public decimal AverageCustomerValue { get; set; }
        public decimal TotalCustomerDebts { get; set; }
        
        public List<Customer> Customers { get; set; } = new List<Customer>();
        
        // أفضل العملاء
        public List<TopCustomerViewModel> TopCustomers { get; set; } = new List<TopCustomerViewModel>();
        
        // إحصائيات العملاء الجدد
        public List<NewCustomerViewModel> NewCustomersByMonth { get; set; } = new List<NewCustomerViewModel>();
    }

    public class FinancialReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal CashBalance { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal WorkingCapital { get; set; }
        public int TotalTransactions { get; set; }
        
        public List<StoreAccount> Transactions { get; set; } = new List<StoreAccount>();
        
        // إحصائيات مالية شهرية
        public List<MonthlyFinancialViewModel> MonthlyFinancials { get; set; } = new List<MonthlyFinancialViewModel>();
        
        // تحليل التدفق النقدي
        public List<CashFlowViewModel> CashFlow { get; set; } = new List<CashFlowViewModel>();
    }


 

    public class GeneralDebtReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        public int TotalDebts { get; set; }
        public decimal TotalDebtAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int ActiveDebts { get; set; }
        public int SettledDebts { get; set; }
        
        public List<GeneralDebt> Debts { get; set; } = new List<GeneralDebt>();
        
        // إحصائيات الديون العامة
        public List<DebtStatusViewModel> DebtStatus { get; set; } = new List<DebtStatusViewModel>();
    }

    public class StoreAccountReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TransactionType { get; set; }
        
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetBalance { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionValue { get; set; }
        
        public List<StoreAccount> Transactions { get; set; } = new List<StoreAccount>();
        
        // إحصائيات حسب نوع العملية
        public List<TransactionTypeViewModel> TransactionTypes { get; set; } = new List<TransactionTypeViewModel>();
        
        // إحصائيات حسب طريقة الدفع
        public List<PaymentMethodViewModel> PaymentMethods { get; set; } = new List<PaymentMethodViewModel>();
    }

    public class UserActivityReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalSales { get; set; }
        public int TotalPurchases { get; set; }
        public int TotalTempMoneyTransactions { get; set; }
        
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        
        // نشاط المستخدمين
        public List<UserActivityViewModel> UserActivities { get; set; } = new List<UserActivityViewModel>();
    }

    public class ProfitLossReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        // الإيرادات
        public decimal SalesRevenue { get; set; }
        public decimal OtherIncome { get; set; }
        public decimal TotalRevenue { get; set; }
        
        // التكاليف
        public decimal CostOfGoodsSold { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal TotalExpenses { get; set; }
        
        // النتائج
        public decimal GrossProfit { get; set; }
        public decimal OperatingProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        
        // تحليل الربحية
        public List<ProfitabilityAnalysisViewModel> ProfitabilityAnalysis { get; set; } = new List<ProfitabilityAnalysisViewModel>();
    }

    public class CategoryPerformanceReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        public int TotalCategories { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalSales { get; set; }
        
        public List<Category> Categories { get; set; } = new List<Category>();
        
        // أداء الفئات
        public List<CategoryPerformanceViewModel> CategoryPerformance { get; set; } = new List<CategoryPerformanceViewModel>();
    }

    // ViewModels مساعدة للتقارير
    public class DailySalesViewModel
    {
        public DateTime Date { get; set; }
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }

    public class CategoryInventoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockCount { get; set; }
    }

    public class TopCustomerViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal OutstandingDebt { get; set; }
    }

    public class NewCustomerViewModel
    {
        public string Month { get; set; }
        public int NewCustomers { get; set; }
    }

    public class MonthlyFinancialViewModel
    {
        public string Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetProfit { get; set; }
    }

    public class CashFlowViewModel
    {
        public string Period { get; set; }
        public decimal CashIn { get; set; }
        public decimal CashOut { get; set; }
        public decimal NetCashFlow { get; set; }
    }

    public class TopSupplierViewModel
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int PurchaseCount { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal OutstandingPayments { get; set; }
    }

    public class MonthlyPurchaseViewModel
    {
        public string Month { get; set; }
        public int PurchaseCount { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class CustomerDebtViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
    }

    public class DebtStatusViewModel
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TransactionTypeViewModel
    {
        public string TransactionType { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PaymentMethodViewModel
    {
        public int? PaymentMethod { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class UserActivityViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int SalesCount { get; set; }
        public int PurchaseCount { get; set; }
        public int TempMoneyCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ProfitabilityAnalysisViewModel
    {
        public string Period { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit { get; set; }
        public decimal Margin { get; set; }
    }

    public class CategoryPerformanceViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
    }
} 