using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Address { get; set; }

        // Multi-tenancy: each customer belongs to a specific tenant
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<CustomerPayment> Payments { get; set; } = new List<CustomerPayment>();
    }
}
