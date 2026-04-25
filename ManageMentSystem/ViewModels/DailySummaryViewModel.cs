using ManageMentSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class DailySummaryViewModel
    {
        public DateTime Date { get; set; } = DateTime.Today;
        
        // إحصائيات المبيعات
        public int TotalSalesCount { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalRemainingAmount { get; set; }
        public decimal TotalSalesProfit { get; set; }
        
        // إحصائيات المبيعات النقدية
        public int CashSalesCount { get; set; }
        public decimal CashSalesAmount { get; set; }
        public decimal CashSalesProfit { get; set; }
        
        // إحصائيات المبيعات المؤجلة
        public int TempMoneyCount { get; set; }
        public decimal TempMoneyAmount { get; set; }
        public decimal TempMoneyPaidAmount { get; set; }
        public decimal TempMoneyRemainingAmount { get; set; }
        public decimal TempMoneyProfit { get; set; }
        
        // إجمالي الإيرادات من حساب المحل (جميع وسائل الدفع)
        public decimal TotalStoreAccountIncome { get; set; }
        public int TotalStoreAccountIncomeTransactions { get; set; }
        
        // إحصائيات الديون العامة
        public int GeneralDebtsCount { get; set; }
        public decimal GeneralDebtsReceivables { get; set; } // دين ليا
        public decimal GeneralDebtsPayables { get; set; } // دين عليا
        public decimal GeneralDebtsNet { get; set; }
        
        // إحصائيات العمليات المالية
        public int StoreAccountTransactionsCount { get; set; }
        public decimal StoreAccountIncome { get; set; }
        public decimal StoreAccountExpenses { get; set; }
        public decimal StoreAccountNet { get; set; }
        
        // إحصائيات المشتريات
        public int PurchaseInvoicesCount { get; set; }
        public decimal PurchaseInvoicesAmount { get; set; }
        public decimal PaidPurchaseInvoicesAmount { get; set; }
        public decimal UnpaidPurchaseInvoicesAmount { get; set; }
        public int UnpaidPurchaseInvoicesCount { get; set; }
        
        // إحصائيات العملاء الجدد
        public int NewCustomersCount { get; set; }
        
        // إحصائيات المنتجات
        public int ProductsSoldCount { get; set; }
        public int LowStockProductsCount { get; set; }
        
        // إحصائيات المرتجعات
        public int ReturnsCount { get; set; }
        public decimal ReturnsTotalAmount { get; set; }
        
        // ملخص شامل
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitPercentage { get; set; }
        
        // قوائم التفاصيل
        public List<Sale> RecentSales { get; set; } = new List<Sale>();
        public List<GeneralDebt> RecentGeneralDebts { get; set; } = new List<GeneralDebt>();
        public List<StoreAccount> RecentStoreAccounts { get; set; } = new List<StoreAccount>();

        public List<Customer> NewCustomers { get; set; } = new List<Customer>();
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        
        // إحصائيات حسب طريقة الدفع
        public List<PaymentMethodBalanceViewModel> PaymentMethodSummaries { get; set; } = new List<PaymentMethodBalanceViewModel>();
        
        // إحصائيات حسب الفئة
        public List<CategorySummary> CategorySummaries { get; set; } = new List<CategorySummary>();
    }
    
}
