using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManageMentSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "سعر الشراء")]
        public decimal PurchasePrice { get; set; }

        [Required]
        [Display(Name = "سعر البيع")]
        public decimal SalePrice { get; set; }

        public int Quantity { get; set; }

        public string? Description { get; set; }

        [Display(Name = "الباركود")]
        public string? Barcode { get; set; }

        // Foreign Key for Category
        [Display(Name = "الفئة")]
        public int? CategoryId { get; set; }

        // Navigation Property
        public virtual Category? Category { get; set; }

        // Multi-tenancy: each product belongs to a specific tenant
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
