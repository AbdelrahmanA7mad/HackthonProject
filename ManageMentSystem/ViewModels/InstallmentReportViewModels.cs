using ManageMentSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    // تقرير الأقساط الشامل
    public class InstallmentReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        
        // إحصائيات عامة
        public int TotalInstallments { get; set; }
        public decimal TotalInstallmentValue { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal TotalInterestAmount { get; set; }
        public decimal AverageInstallmentValue { get; set; }
        public decimal AverageMonthlyPayment { get; set; }
        
        // إحصائيات حسب الحالة
        public int ActiveInstallments { get; set; }
        public int CompletedInstallments { get; set; }
        public int OverdueInstallments { get; set; }
        public int RescheduledInstallments { get; set; }
        
        // إحصائيات مالية
        public decimal ActiveInstallmentsValue { get; set; }
        public decimal CompletedInstallmentsValue { get; set; }
        public decimal OverdueInstallmentsValue { get; set; }
        public decimal RescheduledInstallmentsValue { get; set; }
        
        // البيانات
        public List<Installment> Installments { get; set; } = new List<Installment>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        
        // إحصائيات شهرية
        public List<MonthlyInstallmentViewModel> MonthlyInstallments { get; set; } = new List<MonthlyInstallmentViewModel>();
        
        // أفضل العملاء في الأقساط
        public List<TopInstallmentCustomerViewModel> TopInstallmentCustomers { get; set; } = new List<TopInstallmentCustomerViewModel>();
        
        // إحصائيات حسب عدد الأشهر
        public List<InstallmentDurationViewModel> InstallmentDurations { get; set; } = new List<InstallmentDurationViewModel>();
    }

    // تقرير مدفوعات الأقساط
    public class InstallmentPaymentReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? InstallmentId { get; set; }
        public string? PaymentMethod { get; set; }
        
        // إحصائيات المدفوعات
        public int TotalPayments { get; set; }
        public decimal TotalPaymentAmount { get; set; }
        public decimal AveragePaymentAmount { get; set; }
        public decimal LargestPayment { get; set; }
        public decimal SmallestPayment { get; set; }
        
        // إحصائيات حسب طريقة الدفع
        public decimal CashPayments { get; set; }
        public decimal CardPayments { get; set; }
        public decimal BankTransferPayments { get; set; }
        public decimal OtherPayments { get; set; }
        
        // البيانات
        public List<InstallmentPayment> Payments { get; set; } = new List<InstallmentPayment>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<Installment> Installments { get; set; } = new List<Installment>();
        
        // إحصائيات يومية
        public List<DailyPaymentViewModel> DailyPayments { get; set; } = new List<DailyPaymentViewModel>();
        
        // إحصائيات شهرية
        public List<MonthlyPaymentViewModel> MonthlyPayments { get; set; } = new List<MonthlyPaymentViewModel>();
        
        // إحصائيات حسب طريقة الدفع
        public List<PaymentMethodStatsViewModel> PaymentMethodStats { get; set; } = new List<PaymentMethodStatsViewModel>();
    }

    // تقرير حالة الأقساط
    public class InstallmentStatusReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        // إحصائيات الحالات
        public int TotalInstallments { get; set; }
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int OverdueCount { get; set; }
        public int RescheduledCount { get; set; }
        
        // قيم الحالات
        public decimal ActiveValue { get; set; }
        public decimal CompletedValue { get; set; }
        public decimal OverdueValue { get; set; }
        public decimal RescheduledValue { get; set; }
        
        // نسب الحالات
        public decimal ActivePercentage { get; set; }
        public decimal CompletedPercentage { get; set; }
        public decimal OverduePercentage { get; set; }
        public decimal RescheduledPercentage { get; set; }
        
        // الأقساط المتأخرة
        public List<OverdueInstallmentViewModel> OverdueInstallments { get; set; } = new List<OverdueInstallmentViewModel>();
        
        // الأقساط المجدولة
        public List<RescheduledInstallmentViewModel> RescheduledInstallments { get; set; } = new List<RescheduledInstallmentViewModel>();
        
        // إحصائيات شهرية للحالات
        public List<MonthlyStatusViewModel> MonthlyStatus { get; set; } = new List<MonthlyStatusViewModel>();
    }

    // تقرير أداء الأقساط
    public class InstallmentPerformanceReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        // مؤشرات الأداء
        public decimal CollectionRate { get; set; } // معدل التحصيل
        public decimal OverdueRate { get; set; } // معدل التأخير
        public decimal CompletionRate { get; set; } // معدل الإكمال
        public decimal RescheduleRate { get; set; } // معدل إعادة الجدولة
        
        // متوسطات
        public decimal AverageCollectionTime { get; set; } // متوسط وقت التحصيل بالأيام
        public decimal AverageOverdueDays { get; set; } // متوسط أيام التأخير
        public decimal AverageCompletionTime { get; set; } // متوسط وقت الإكمال بالأشهر
        
        // إحصائيات مالية
        public decimal TotalExpectedRevenue { get; set; }
        public decimal TotalActualRevenue { get; set; }
        public decimal RevenueEfficiency { get; set; } // كفاءة الإيرادات
        
        // تحليل الاتجاهات
        public List<MonthlyPerformanceViewModel> MonthlyPerformance { get; set; } = new List<MonthlyPerformanceViewModel>();
        
        // تحليل العملاء
        public List<CustomerPerformanceViewModel> CustomerPerformance { get; set; } = new List<CustomerPerformanceViewModel>();
    }

    // ViewModels مساعدة
    public class MonthlyInstallmentViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int InstallmentCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Outstanding { get; set; }
    }

    public class TopInstallmentCustomerViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int InstallmentCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Outstanding { get; set; }
        public decimal AverageInstallmentValue { get; set; }
    }

    public class InstallmentDurationViewModel
    {
        public int DurationMonths { get; set; }
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
        public decimal AverageValue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class DailyPaymentViewModel
    {
        public DateTime Date { get; set; }
        public int PaymentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal CardAmount { get; set; }
        public decimal BankTransferAmount { get; set; }
    }

    public class MonthlyPaymentViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int PaymentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    public class PaymentMethodStatsViewModel
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class OverdueInstallmentViewModel
    {
        public int InstallmentId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal OutstandingAmount { get; set; }
        public DateTime StartDate { get; set; }
        public int OverdueDays { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class RescheduledInstallmentViewModel
    {
        public int InstallmentId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidBeforeReschedule { get; set; }
        public DateTime OriginalStartDate { get; set; }
        public DateTime RescheduleDate { get; set; }
        public int RescheduleCount { get; set; }
    }

    public class MonthlyStatusViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }
        public int OverdueCount { get; set; }
        public int RescheduledCount { get; set; }
    }

    public class MonthlyPerformanceViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal CollectionRate { get; set; }
        public decimal OverdueRate { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal RevenueEfficiency { get; set; }
    }

    public class CustomerPerformanceViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int TotalInstallments { get; set; }
        public int CompletedInstallments { get; set; }
        public int OverdueInstallments { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal OverdueRate { get; set; }
        public decimal AveragePaymentTime { get; set; }
    }
}
