using Microsoft.AspNetCore.Identity;

namespace ManageMentSystem.Models
{
    /// <summary>
    /// المالك (Owner) - يستخدم ASP.NET Identity ولديه جميع الصلاحيات تلقائياً
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Preferred currency for formatting (e.g., EGP, SAR)
        public string? PreferredCurrency { get; set; }

        // Multi-tenancy: Owner/User belongs to ONE Tenant
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
