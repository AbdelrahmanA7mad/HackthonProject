using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [Display(Name = "اسم المستخدم أو البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [Display(Name = "تذكرني")]
        public bool RememberMe { get; set; }

        // معرف الجهاز - مخفي من المستخدم
        public string? DeviceId { get; set; }
        
        // بصمة الجهاز الكاملة - مخفي من المستخدم
        public string? DeviceFingerprint { get; set; }
    }
} 