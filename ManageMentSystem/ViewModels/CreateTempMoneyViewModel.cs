using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CreateTempMoneyViewModel
    {
        [Display(Name = "العميل")]
        public int CustomerId { get; set; }
        
        // New customer properties
        [Display(Name = "اسم العميل الجديد")]
        public string? NewCustomerName { get; set; }
        
        [Display(Name = "رقم هاتف العميل الجديد")]
        public string? NewCustomerPhone { get; set; }
        
        [Display(Name = "عنوان العميل الجديد")]
        public string? NewCustomerAddress { get; set; }

        [Required(ErrorMessage = "تاريخ الاستحقاق مطلوب")]
        [Display(Name = "تاريخ الاستحقاق")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "المبلغ الإجمالي مطلوب")]
        [Display(Name = "المبلغ الإجمالي")]
        [Range(0, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "المبلغ المدفوع مطلوب")]
        [Display(Name = "المبلغ المدفوع")]
        [Range(0, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal PaidAmount { get; set; }

        public List<TempMoneyItemViewModel>? TempMoneyItems { get; set; } = new List<TempMoneyItemViewModel>();

        [Display(Name = "طريقة الدفع")]
        public int? PaymentMethodId { get; set; }
    }

    public class TempMoneyItemViewModel
    {
        [Display(Name = "المنتج")]
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Display(Name = "الكمية")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "سعر الوحدة مطلوب")]
        [Display(Name = "سعر الوحدة")]
        [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal UnitPrice { get; set; }

        // سعر البيع المخصص (يمكن تغييره أثناء البيع)
        [Display(Name = "سعر البيع المخصص")]
        [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal CustomSalePrice { get; set; }

        // سعر الشراء للمقارنة
        [Display(Name = "سعر الشراء")]
        public decimal PurchasePrice { get; set; }

        // Additional properties for display
        public string? ProductName { get; set; }
        public decimal SubTotal => Quantity * UnitPrice;
        
        // حساب الربح أو الخسارة
        public decimal Profit => (UnitPrice - PurchasePrice) * Quantity;
        
        // حالة الربح أو الخسارة
        public string ProfitStatus => Profit > 0 ? "ربح" : Profit < 0 ? "خسارة" : "تعادل";
        
        [Display(Name = "الوصف")]
        public string? Description { get; set; }
    }
}
