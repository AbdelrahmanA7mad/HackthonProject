using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CreateSupplierViewModel
    {
        [Required(ErrorMessage = "اسم المورد مطلوب")]
        [Display(Name = "اسم المورد")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string? Email { get; set; }

        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }
    }

    public class EditSupplierViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المورد مطلوب")]
        [Display(Name = "اسم المورد")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string? Email { get; set; }

        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; }
    }

    public class SupplierDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
