using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.Models
{
    public class CustomerPayment
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public int? PaymentMethodId { get; set; }
        public PaymentMethodOption? PaymentMethod { get; set; }

        // Multi-tenancy
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }
        public string? Notes { get; set; }

        public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
    }

    public class PaymentAllocation
    {
        public int Id { get; set; }

        [Required]
        public int CustomerPaymentId { get; set; }
        public CustomerPayment CustomerPayment { get; set; } = null!;

        [Required]
        public int SaleId { get; set; }
        public Sale Sale { get; set; } = null!;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        // Multi-tenancy: inherited from parent CustomerPayment
        public string? TenantId { get; set; }
    }
}
