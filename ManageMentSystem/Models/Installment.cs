using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManageMentSystem.Models
{
    public class Installment
    {
        public int Id { get; set; }

        [Display(Name = "رقم التقسيط")]
        public int SequenceNumber { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public decimal DownPayment { get; set; }

        public decimal MonthlyPayment { get; set; }

        public int NumberOfMonths { get; set; }

        public DateTime StartDate { get; set; }

        public string Status { get; set; } // "نشط", "مكتمل", "متأخر"

        public DateTime? CompletionDate { get; set; }

        // Interest fields
        public decimal InterestRate { get; set; } // Interest rate percentage (e.g., 5.5 for 5.5%)
        
        public decimal InterestAmount { get; set; } // Total interest amount
        
        public decimal TotalWithInterest { get; set; } // Total amount including interest
        
        public decimal RemainingAmount { get; set; } // Remaining amount to be paid
        
        public decimal TotalPaid { get; set; } // Total amount paid so far

        // Extra month field for additional month feature
        public bool HasExtraMonth { get; set; } = false; // Whether the installment has an extra month
        public decimal ExtraMonthAmount { get; set; } = 0; // Amount for the extra month

        // Reschedule tracking
        public DateTime? RescheduleDate { get; set; } // When the installment was rescheduled
        public decimal TotalPaidBeforeReschedule { get; set; } = 0; // Total paid before rescheduling

        // Navigation property for payments
        public ICollection<InstallmentPayment> Payments { get; set; } = new List<InstallmentPayment>();

        // إضافة مجموعة العناصر
        public ICollection<InstallmentItem> InstallmentItems { get; set; } = new List<InstallmentItem>();

        // Multi-tenancy
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Guarantor fields
        [Display(Name = "اسم الضامن")]
        public string? GuarantorName { get; set; }

        [Display(Name = "رقم هاتف الضامن")]
        public string? GuarantorPhone { get; set; }
    }
}
