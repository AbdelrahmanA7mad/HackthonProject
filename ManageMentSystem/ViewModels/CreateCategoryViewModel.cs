using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "اسم الفئة مطلوب")]
        [Display(Name = "اسم الفئة")]
        public string Name { get; set; }

        [Display(Name = "الوصف")]
        public string? Description { get; set; }
    }
} 