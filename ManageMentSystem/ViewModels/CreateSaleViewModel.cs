using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CreateSaleViewModel
    {
        [Display(Name = "العميل")]
        public int? CustomerId { get; set; }
        
        // New customer properties
        [Display(Name = "اسم العميل الجديد")]
        public string? NewCustomerName { get; set; }
        
        [Display(Name = "رقم هاتف العميل الجديد")]
        public string? NewCustomerPhone { get; set; }
        
        [Display(Name = "عنوان العميل الجديد")]
        public string? NewCustomerAddress { get; set; }

        [Required(ErrorMessage = "تاريخ البيع مطلوب")]
        [Display(Name = "تاريخ البيع")]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "المبلغ الإجمالي مطلوب")]
        [Display(Name = "المبلغ الإجمالي")]
        [Range(0, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal TotalAmount { get; set; }

        // حقول الخصم
        [Display(Name = "نسبة الخصم %")]
        [Range(0, 100, ErrorMessage = "نسبة الخصم يجب أن تكون بين 0 و 100")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "مبلغ الخصم")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ الخصم يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal DiscountAmount { get; set; } = 0;

        // المبلغ بعد الخصم
        [Display(Name = "المبلغ بعد الخصم")]
        public decimal AmountAfterDiscount => TotalAmount - DiscountAmount;

        // المبلغ المدفوع يمكن أن يكون جزئيًا (للبيع الآجل/الجزئي)
        [Display(Name = "المبلغ المدفوع")]
        [Range(0, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal PaidAmount { get; set; }

        public List<SaleItemViewModel>? SaleItems { get; set; } = new List<SaleItemViewModel>();

        [Display(Name = "طريقة الدفع")]
        public int? PaymentMethodId { get; set; }

        [Display(Name = "نوع الدفع")]
        public ManageMentSystem.Models.SalePaymentType PaymentType { get; set; } = ManageMentSystem.Models.SalePaymentType.Cash;
    }

    public class SaleItemViewModel
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
    }
} 