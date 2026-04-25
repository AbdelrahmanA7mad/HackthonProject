using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CreateReturnViewModel
    {
        public int? CustomerId { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        [Required]
        public List<CreateReturnItemViewModel> Items { get; set; } = new();

        public string? Notes { get; set; }

        // نوع خصم المرتجع
        public ReturnDeductionType DeductionType { get; set; } = ReturnDeductionType.Auto;

        public decimal TotalAmount => Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0m;
    }

    public enum ReturnDeductionType
    {
        Auto = 0,           // تلقائي (حسب رصيد العميل)
        FromCustomer = 1,   // خصم من رصيد العميل
        FromStore = 2       // خصم من حساب المحل
    }

    public class CreateReturnItemViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}


