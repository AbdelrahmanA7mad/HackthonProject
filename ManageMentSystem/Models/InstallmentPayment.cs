using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.Models
{
    public class InstallmentPayment
    {
        public int Id { get; set; }

        public int InstallmentId { get; set; }
        public Installment Installment { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        public int? PaymentMethodId { get; set; }
        public PaymentMethodOption? PaymentMethod { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Multi-tenancy: inherited from parent Installment
        public string? TenantId { get; set; }


    }
}