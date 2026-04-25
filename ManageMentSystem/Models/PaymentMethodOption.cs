using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.Models
{
    public class PaymentMethodOption
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "طريقة الدفع")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "افتراضي")]
        public bool IsDefault { get; set; } = false;

        [Display(Name = "ترتيب العرض")]
        public int SortOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Multi-tenancy
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

    }
}
