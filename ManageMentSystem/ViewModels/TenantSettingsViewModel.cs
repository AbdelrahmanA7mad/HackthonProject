using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class TenantSettingsViewModel
    {
        [Required(ErrorMessage = "اسم المؤسسة مطلوب")]
        [Display(Name = "اسم المؤسسة")]
        [StringLength(100, ErrorMessage = "اسم المؤسسة لا يجب أن يتجاوز 100 حرف")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "العنوان")]
        [StringLength(200, ErrorMessage = "العنوان لا يجب أن يتجاوز 200 حرف")]
        public string? Address { get; set; }

        [Display(Name = "رقم الهاتف")]
        [StringLength(50, ErrorMessage = "رقم الهاتف لا يجب أن يتجاوز 50 حرف")]
        public string? Phone { get; set; }

        [Display(Name = "رابط الشعار")]
        [StringLength(200, ErrorMessage = "رابط الشعار لا يجب أن يتجاوز 200 حرف")]
        public string? LogoUrl { get; set; }

        [Required(ErrorMessage = "رمز العملة مطلوب")]
        [Display(Name = "رمز العملة")]
        [StringLength(10, ErrorMessage = "رمز العملة لا يجب أن يتجاوز 10 أحرف")]
        public string CurrencyCode { get; set; } = "EGP";

        // للعرض فقط (غير قابلة للتعديل من المستخدم العادي)
        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "حالة الاشتراك")]
        public string SubscriptionStatus { get; set; } = "Trial";

        [Display(Name = "تاريخ انتهاء الفترة التجريبية")]
        public DateTime? TrialEndDate { get; set; }

        [Display(Name = "تاريخ انتهاء الاشتراك")]
        public DateTime? SubscriptionEndDate { get; set; }

        [Display(Name = "الحالة")]
        public bool IsActive { get; set; } = true;
    }
}
