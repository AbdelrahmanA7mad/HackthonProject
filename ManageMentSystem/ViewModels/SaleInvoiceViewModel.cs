using ManageMentSystem.Models;

namespace ManageMentSystem.ViewModels
{
    public class SaleInvoiceViewModel
    {
        public Sale Sale { get; set; } = null!;
        public Invoice? CompanySettings { get; set; }
    }
}

