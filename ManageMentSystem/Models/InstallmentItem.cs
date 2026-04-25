using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.Models
{
    public class InstallmentItem
    {
        public int Id { get; set; }

        [Required]
        public int InstallmentId { get; set; }
        public Installment Installment { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        [Display(Name = "الكمية")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "سعر الوحدة")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Display(Name = "إجمالي السعر")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Display(Name = "اسم المنتج")]
        public string? ProductName { get; set; }

        // Multi-tenancy: inherited from parent Installment
        public string? TenantId { get; set; }
    }
}
