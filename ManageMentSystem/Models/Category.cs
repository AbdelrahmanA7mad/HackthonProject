using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "اسم الفئة")]
        public string Name { get; set; }

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        // Multi-tenancy: each category belongs to a specific tenant
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }


        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}