using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.Models
{
    public class InventorySettings
    {
        public int Id { get; set; }
        
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        [Required]
        [Display(Name = "تفعيل تنبيهات المخزون الناقص")]
        public bool EnableLowStockAlerts { get; set; } = true;

        [Required]
        [Range(1, 1000, ErrorMessage = "يجب أن تكون الكمية بين 1 و 1000")]
        [Display(Name = "كمية المخزون الناقص")]
        public int LowStockThreshold { get; set; } = 10;

        [Display(Name = "رسالة التنبيه")]
        public string? AlertMessage { get; set; } = "تنبيه: المنتج {0} وصل إلى الحد الأدنى للمخزون ({1} وحدة)";

        [Display(Name = "إرسال تنبيه عند الوصول للحد الأدنى")]
        public bool ShowAlertOnThreshold { get; set; } = true;

        [Display(Name = "إظهار تنبيه في لوحة التحكم")]
        public bool ShowDashboardAlert { get; set; } = true;

        [Display(Name = "إظهار تنبيه في صفحة المنتجات")]
        public bool ShowProductsPageAlert { get; set; } = true;

        [Display(Name = "إظهار تنبيه في التقارير")]
        public bool ShowReportsAlert { get; set; } = true;

        [Display(Name = "لون التنبيه")]
        public string AlertColor { get; set; } = "warning";

        [Display(Name = "أيقونة التنبيه")]
        public string AlertIcon { get; set; } = "alert-triangle";
    }
}
