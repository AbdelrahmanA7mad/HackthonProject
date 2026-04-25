using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CustomerReceivableEntry
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance => Math.Max(0, TotalSales - TotalPaid);
    }

    public class ReceivablesReportViewModel
    {
        [Display(Name = "إجمالي مستحقات العملاء")] public decimal TotalReceivables { get; set; }
        public List<CustomerReceivableEntry> Entries { get; set; } = new();
    }

    public class SupplierPayableEntry
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalPurchases { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance => Math.Max(0, TotalPurchases - TotalPaid);
    }

    public class PayablesReportViewModel
    {
        [Display(Name = "إجمالي التزامات الموردين")] public decimal TotalPayables { get; set; }
        public List<SupplierPayableEntry> Entries { get; set; } = new();
    }
}


