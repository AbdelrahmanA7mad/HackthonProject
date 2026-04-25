using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CreateInstallmentViewModel
    {
        [Display(Name = "العميل")]
        public int CustomerId { get; set; }

        // إزالة ProductId الفردي لأننا سنستخدم العناصر المتعددة
        // [Display(Name = "المنتج")]
        // public int? ProductId { get; set; }

        [Required(ErrorMessage = "المبلغ الإجمالي مطلوب")]
        [Display(Name = "المبلغ الإجمالي")]
        [Range(0, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "الدفعة المقدمة مطلوبة")]
        [Display(Name = "الدفعة المقدمة")]
        [Range(0, double.MaxValue, ErrorMessage = "الدفعة المقدمة يجب أن تكون أكبر من أو تساوي صفر")]
        public decimal DownPayment { get; set; }

        [Required(ErrorMessage = "الدفعة الشهرية مطلوبة")]
        [Display(Name = "الدفعة الشهرية")]
        [Range(0, double.MaxValue, ErrorMessage = "الدفعة الشهرية يجب أن تكون أكبر من صفر")]
        public decimal MonthlyPayment { get; set; }

        [Required(ErrorMessage = "عدد الأشهر مطلوب")]
        [Display(Name = "عدد الأشهر")]
        [Range(1, int.MaxValue, ErrorMessage = "عدد الأشهر يجب أن يكون أكبر من صفر")]
        public int NumberOfMonths { get; set; }

        [Required(ErrorMessage = "تاريخ البدء مطلوب")]
        [Display(Name = "تاريخ البدء")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "نسبة الفائدة مطلوبة")]
        [Display(Name = "نسبة الفائدة (%)")]
        [Range(0, 100, ErrorMessage = "نسبة الفائدة يجب أن تكون بين 0 و 100")]
        public decimal InterestRate { get; set; }

        // Extra month properties
        [Display(Name = "إضافة شهر إضافي")]
        public bool HasExtraMonth { get; set; } = false;

        [Display(Name = "مبلغ الشهر الإضافي")]
        public decimal ExtraMonthAmount { get; set; } = 0;

        // Additional properties for display and calculations
        public string? CustomerName { get; set; }
        // public string? ProductName { get; set; } // إزالة لأننا نستخدم العناصر المتعددة
        
        // New customer properties
        [Display(Name = "اسم العميل الجديد")]
        public string? NewCustomerName { get; set; }
        
        [Display(Name = "رقم هاتف العميل الجديد")]
        public string? NewCustomerPhone { get; set; }
        
        [Display(Name = "عنوان العميل الجديد")]
        public string? NewCustomerAddress { get; set; }

        // Guarantor properties
        [Display(Name = "اسم الضامن")]
        public string? GuarantorName { get; set; }

        [Display(Name = "رقم هاتف الضامن")]
        public string? GuarantorPhone { get; set; }
        
        // Calculated properties
        public decimal RemainingAmount => TotalAmount - DownPayment;
        public decimal InterestAmount => RemainingAmount * (InterestRate / 100) * (NumberOfMonths / 12m);
        public decimal TotalWithInterest => TotalAmount + InterestAmount;
        public decimal TotalPayments => DownPayment + (MonthlyPayment * NumberOfMonths);
        public decimal MonthlyPaymentWithInterest => (RemainingAmount + InterestAmount) / NumberOfMonths;
        
        // Extra month calculations
        public decimal TotalWithExtraMonth => TotalWithInterest + ExtraMonthAmount;
        public decimal TotalPaymentsWithExtraMonth => TotalPayments + ExtraMonthAmount;

        // قائمة العناصر
        public List<InstallmentItemViewModel> Items { get; set; } = new List<InstallmentItemViewModel>();

        [Display(Name = "طريقة الدفع")]
        public int? PaymentMethodId { get; set; }

        [Display(Name = "نوع الدفع")]
        public ManageMentSystem.Models.SalePaymentType PaymentType { get; set; } = ManageMentSystem.Models.SalePaymentType.Cash;
    }

    public class InstallmentItemViewModel
    {
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Description { get; set; }
    }
    
} 