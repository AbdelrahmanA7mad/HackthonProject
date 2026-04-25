using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class PaymentMethodBalanceViewModel
    {
        public int? PaymentMethodId { get; set; }

        [Display(Name = "طريقة الدفع")]
        public string Name { get; set; } = "غير محدد";

        [Display(Name = "إجمالي الإيرادات")]
        public decimal TotalIncome { get; set; }

        [Display(Name = "إجمالي المصروفات")]
        public decimal TotalExpenses { get; set; }

        [Display(Name = "الرصيد الصافي")]
        public decimal Net => TotalIncome - TotalExpenses;

        [Display(Name = "عدد العمليات")]
        public int TransactionCount { get; set; }
    }
}

