using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CreatePurchaseInvoiceViewModel
    {
        [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
        [Display(Name = "رقم الفاتورة")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الفاتورة مطلوب")]
        [Display(Name = "تاريخ الفاتورة")]
        [DataType(DataType.Date)]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "المورد مطلوب")]
        [Display(Name = "المورد")]
        public int SupplierId { get; set; }

        [Display(Name = "مبلغ الخصم")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ الخصم يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "نسبة الخصم")]
        [Range(0, 100, ErrorMessage = "نسبة الخصم يجب أن تكون بين 0 و 100")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Display(Name = "المدفوع")]
        [Range(0, double.MaxValue, ErrorMessage = "المبلغ المدفوع يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal PaidAmount { get; set; } = 0;

        [Display(Name = "تاريخ الاستحقاق")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [Display(Name = "طريقة الدفع")]
        public int? PaymentMethodId { get; set; }

        // عناصر الفاتورة
        public List<CreatePurchaseInvoiceItemViewModel> Items { get; set; } = new List<CreatePurchaseInvoiceItemViewModel>();
    }

    public class CreatePurchaseInvoiceItemViewModel
    {
        [Display(Name = "المنتج")]
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Display(Name = "الكمية")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "سعر الوحدة مطلوب")]
        [Display(Name = "سعر الوحدة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر الوحدة يجب أن يكون أكبر من صفر")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "الخصم")]
        [Range(0, double.MaxValue, ErrorMessage = "الخصم يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal DiscountAmount { get; set; } = 0;

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // خصائص المنتج الجديد
        [Display(Name = "منتج جديد")]
        public bool IsNewProduct { get; set; } = false;

        [Display(Name = "اسم المنتج الجديد")]
        public string? NewProductName { get; set; }

        [Display(Name = "باركود المنتج الجديد")]
        public string? NewProductBarcode { get; set; }

        [Display(Name = "فئة المنتج الجديد")]
        public int? NewProductCategoryId { get; set; }

        [Display(Name = "وصف المنتج الجديد")]
        public string? NewProductDescription { get; set; }

        [Display(Name = "سعر بيع المنتج الجديد")]
        public decimal NewProductSalePrice { get; set; }
    }

    public class EditPurchaseInvoiceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "رقم الفاتورة مطلوب")]
        [Display(Name = "رقم الفاتورة")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ الفاتورة مطلوب")]
        [Display(Name = "تاريخ الفاتورة")]
        [DataType(DataType.Date)]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "المورد مطلوب")]
        [Display(Name = "المورد")]
        public int SupplierId { get; set; }

        [Display(Name = "مبلغ الخصم")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ الخصم يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "نسبة الخصم")]
        [Range(0, 100, ErrorMessage = "نسبة الخصم يجب أن تكون بين 0 و 100")]
        public decimal DiscountPercentage { get; set; }

        [Display(Name = "المدفوع")]
        [Range(0, double.MaxValue, ErrorMessage = "المبلغ المدفوع يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "تاريخ الاستحقاق")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        [Display(Name = "طريقة الدفع")]
        public int? PaymentMethodId { get; set; }

        // عناصر الفاتورة
        public List<EditPurchaseInvoiceItemViewModel> Items { get; set; } = new List<EditPurchaseInvoiceItemViewModel>();
    }

    public class EditPurchaseInvoiceItemViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "المنتج مطلوب")]
        [Display(Name = "المنتج")]
        public int ProductId { get; set; }

        [Display(Name = "اسم المنتج")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Display(Name = "الكمية")]
        [Range(1, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "سعر الوحدة مطلوب")]
        [Display(Name = "سعر الوحدة")]
        [Range(0.01, double.MaxValue, ErrorMessage = "سعر الوحدة يجب أن يكون أكبر من صفر")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "الخصم")]
        [Range(0, double.MaxValue, ErrorMessage = "الخصم يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }
    }

    public class PurchaseInvoiceDetailsViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AmountAfterDiscount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string? Notes { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // معلومات المورد
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierPhone { get; set; } = string.Empty;
        public string SupplierEmail { get; set; } = string.Empty;

        // معلومات المستخدم
        public string CreatedByUserName { get; set; } = string.Empty;

        // عناصر الفاتورة
        public List<PurchaseInvoiceItemDetailsViewModel> Items { get; set; } = new List<PurchaseInvoiceItemDetailsViewModel>();
    }

    public class PurchaseInvoiceItemDetailsViewModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string? Notes { get; set; }

        // معلومات المنتج
        public string ProductName { get; set; } = string.Empty;
        public string? ProductBarcode { get; set; }
        public string? ProductDescription { get; set; }
    }

    public enum PaymentStatus
    {
        [Display(Name = "مدفوع بالكامل")]
        Paid = 1,
        [Display(Name = "مدفوع جزئياً")]
        Partial = 2,
        [Display(Name = "غير مدفوع")]
        Unpaid = 3
    }
}
