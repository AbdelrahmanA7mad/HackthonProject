using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManageMentSystem.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "اسم الشركة")]
        public string CompanyName { get; set; } = "شركة غير محددة";

        [Display(Name = "العنوان الفرعي")]
        public string? CompanySubtitle { get; set; } = "خدمات الفواتير";

        [Display(Name = "أرقام الهاتف")]
        public List<string> PhoneNumbers { get; set; } = new List<string> { "01000000000", "01100000000" };

        [Display(Name = "العنوان")]
        public string? Address { get; set; } = "القاهرة، مصر";

        [Display(Name = "رسالة التذييل")]
        public string? FooterMessage { get; set; } = "شكرًا لتعاملكم معنا";

        [Display(Name = "شعار الشركة")]
        public string? Logo { get; set; } = "/images/default-logo.png";

        [Display(Name = "الموقع الإلكتروني")]
        public string? Website { get; set; } = "https://example.com";

        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; } = "info@example.com";

        [Display(Name = "رقم المستخدم")]
        public string UserId { get; set; } = string.Empty;
        [ValidateNever]
        public ApplicationUser User { get; set; } = null!;

        // Multi-tenancy
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
