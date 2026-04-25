using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.Models
{
    public class TabVisibilitySettings
    {
        public int Id { get; set; }
        
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        [Display(Name = "إظهار المنتجات")]
        public bool ShowProducts { get; set; } = true;

        [Display(Name = "إظهار الفئات")]
        public bool ShowCategories { get; set; } = true;

        [Display(Name = "إظهار العملاء")]
        public bool ShowCustomers { get; set; } = true;

        [Display(Name = "إظهار المبيعات")]
        public bool ShowSales { get; set; } = true;

        [Display(Name = "إظهار المرتجعات")]
        public bool ShowReturns { get; set; } = true;

        [Display(Name = "إظهار الأقساط")]
        public bool ShowInstallments { get; set; } = true;

        [Display(Name = "إظهار الديون")]
        public bool ShowGeneralDebts { get; set; } = true;

        [Display(Name = "إظهار الموردين")]
        public bool ShowSuppliers { get; set; } = true;

        [Display(Name = "إظهار فواتير المشتريات")]
        public bool ShowPurchaseInvoices { get; set; } = true;

        [Display(Name = "إظهار الحسابات")]
        public bool ShowStoreAccount { get; set; } = true;


        [Display(Name = "إظهار التقارير")]
        public bool ShowReports { get; set; } = true;
    }
}
