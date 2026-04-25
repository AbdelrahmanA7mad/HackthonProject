using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CreateReturnFromSaleViewModel
    {
        [Required]
        public int SaleId { get; set; }

        public int? CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        public List<SaleItemForReturnViewModel> SaleItems { get; set; } = new();

        public string? Notes { get; set; }

        // نوع خصم المرتجع
        public ReturnDeductionType DeductionType { get; set; } = ReturnDeductionType.Auto;

        public decimal TotalAmount => SaleItems?.Where(si => si.IsSelected).Sum(si => si.ReturnQuantity * si.UnitPrice) ?? 0m;
    }

    public class SaleItemForReturnViewModel
    {
        public int SaleItemId { get; set; }
        
        public int ProductId { get; set; }
        
        public string ProductName { get; set; } = string.Empty;
        
        public string? Barcode { get; set; }
        
        public int OriginalQuantity { get; set; }
        
        public int ReturnQuantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public bool IsSelected { get; set; }
        
        public decimal TotalPrice => ReturnQuantity * UnitPrice;
    }
}

