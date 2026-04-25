using ManageMentSystem.Models;

namespace ManageMentSystem.ViewModels
{
    public class InstallmentDetailsViewModel
    {
        public Installment Installment { get; set; } = null!;
        public List<MonthlyPaymentStatus> MonthlyPayments { get; set; } = new List<MonthlyPaymentStatus>();
        public List<InstallmentItem> Items { get; set; } = new List<InstallmentItem>();
        public Invoice? CompanySettings { get; set; }
        
        // معلومات الدفعات السابقة (قبل إعادة الجدولة)
        public decimal TotalPaidBeforeReschedule { get; set; }
        public int PaidMonthsBeforeReschedule { get; set; }
        public bool IsRescheduled { get; set; }
        public decimal OriginalMonthlyPayment { get; set; }
        public int OriginalNumberOfMonths { get; set; }
    }

    public class MonthlyPaymentStatus
    {
        public int MonthNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? PaidAmount { get; set; }
        public string Status { get; set; } = string.Empty; // "مدفوع", "غير مدفوع", "متأخر"
        public bool IsExtraMonth { get; set; } = false; // Whether this is the extra month
    }
} 