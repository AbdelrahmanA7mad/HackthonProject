using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.Models
{
    public class StoreAccount
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "اسم العملية")]
        public string TransactionName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "نوع العملية")]
        public TransactionType TransactionType { get; set; }

        [Required]
        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }

        [Display(Name = "التاريخ")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Display(Name = "الفئة")]
        public string? Category { get; set; }

        [Display(Name = "طريقة الدفع")]
        public int? PaymentMethodId { get; set; }
        public PaymentMethodOption? PaymentMethod { get; set; }

        [Display(Name = "مرجع العملية")]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [Display(Name = "رأس المال")]
        public decimal Capital { get; set; } = 0;

        // Foreign Keys for related entities
        public int? SaleId { get; set; }
        public Sale? Sale { get; set; }

        public int? TempMoneyId { get; set; }

        public int? GeneralDebtId { get; set; }
        public GeneralDebt? GeneralDebt { get; set; }

        public int? InstallmentPaymentId { get; set; }
        public InstallmentPayment? InstallmentPayment { get; set; }

        // Multi-tenancy
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum TransactionType
    {
        [Display(Name = "إيراد")]
        Income,
        [Display(Name = "مصروف")]
        Expense
    }

} 