using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CapitalSummaryViewModel
    {
        // Components
        [Display(Name = "رصيد الخزنة")] public decimal StoreNetBalance { get; set; }
        [Display(Name = "مستحقات العملاء")] public decimal CustomerReceivables { get; set; }
        [Display(Name = "ديون عامة لصالحي")] public decimal GeneralReceivables { get; set; }
        [Display(Name = "التزامات الموردين")] public decimal SupplierPayables { get; set; }
        [Display(Name = "ديون عامة عليّ")] public decimal GeneralPayables { get; set; }
        [Display(Name = "قيمة المخزون")] public decimal InventoryValue { get; set; }

        // Aggregates
        [Display(Name = "إجمالي المبالغ المستحقة")] public decimal TotalReceivables => CustomerReceivables + GeneralReceivables;
        [Display(Name = "إجمالي الالتزامات")] public decimal TotalPayables => SupplierPayables + GeneralPayables;
        // للإبقاء على التوافق مع الواجهات التي تستخدم TotalLiabilities
        public decimal TotalLiabilities => TotalPayables;
        [Display(Name = "إجمالي الأصول")] public decimal TotalAssets => StoreNetBalance + TotalReceivables + InventoryValue;
        [Display(Name = "صافي رأس المال")] public decimal NetCapital => TotalAssets - TotalLiabilities;
    }
}


