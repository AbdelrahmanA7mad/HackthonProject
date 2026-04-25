using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ManageMentSystem.ViewModels
{
    public class CreateProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [Display(Name = "اسم المنتج")]
        public string Name { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون صفر أو أكثر")]
        [Display(Name = "الكمية")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "سعر الشراء مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر الشراء يجب أن يكون أكبر من صفر")]
        [Display(Name = "سعر الشراء")]
        public decimal PurchasePrice { get; set; }

        [Display(Name = "سعر البيع")]
        public decimal SalePrice { get; set; }

        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Display(Name = "الباركود")]
        [Remote(action: "CheckBarcodeUnique", controller: "Products", AdditionalFields = "Id", ErrorMessage = "هذا الباركود مستخدم بالفعل في منتج آخر")]
        public string? Barcode { get; set; }

        [Required(ErrorMessage = "الفئة مطلوبة")]
        [Display(Name = "الفئة")]
        public int? CategoryId { get; set; }
    }
}
