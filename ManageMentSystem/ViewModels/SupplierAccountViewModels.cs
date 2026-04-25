using System.ComponentModel.DataAnnotations;
using ManageMentSystem.Models;

namespace ManageMentSystem.ViewModels
{
    public class SupplierPaymentInputViewModel
    {
        [Required]
        public int SupplierId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime? PaymentDate { get; set; }

        public int? PaymentMethodId { get; set; }

        public string? Notes { get; set; }
    }

    public class SupplierAccountEntry
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "فاتورة" أو "دفعة"
        public decimal Amount { get; set; }
        public int? PurchaseInvoiceId { get; set; }
        public int? PaymentId { get; set; }
        public string? Notes { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal RunningPurchases { get; set; }
        public decimal RunningPaid { get; set; }
        public decimal RunningBalance { get; set; }
        public List<SupplierPaymentAllocationSummary> Allocations { get; set; } = new();
    }

    public class SupplierFullAccountViewModel
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierPhone { get; set; }
        public List<SupplierAccountEntry> Entries { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public decimal TotalPurchases { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }
    }

    public class SupplierPaymentAllocationSummary
    {
        public int PurchaseInvoiceId { get; set; }
        public decimal Amount { get; set; }
    }
}


