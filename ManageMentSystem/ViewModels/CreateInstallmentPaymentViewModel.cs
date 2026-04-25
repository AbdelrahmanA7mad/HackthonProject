using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CreateInstallmentPaymentViewModel
    {
        public int InstallmentId { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Display(Name = "المبلغ")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "تاريخ الدفع مطلوب")]
        [Display(Name = "تاريخ الدفع")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // Additional properties for display
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainingAmount { get; set; }
        public int NumberOfMonths { get; set; }
        public int PaidMonths { get; set; }
        public int RemainingMonths { get; set; }
        public int NextMonthToPay { get; set; }
        public DateTime NextMonthDueDate { get; set; }

        [Display(Name = "طريقة الدفع")]
        public int? PaymentMethodId { get; set; }

    }
} 