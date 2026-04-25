using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManageMentSystem.Models
{
    public class Tenant
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? LogoUrl { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(10)]
        public string CurrencyCode { get; set; } = "EGP";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Subscription Management
        public DateTime? TrialEndDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        
        [StringLength(20)]
        public string SubscriptionStatus { get; set; } = "Trial"; // Trial, Active, Expired

        // Computed property to check if subscription is active
        [NotMapped]
        public bool IsSubscriptionActive
        {
            get
            {
                var now = DateTime.Now;
                
                // Check trial period
                if (SubscriptionStatus == "Trial" && TrialEndDate.HasValue && TrialEndDate.Value > now)
                    return true;
                
                // Check paid subscription
                if (SubscriptionStatus == "Active" && SubscriptionEndDate.HasValue && SubscriptionEndDate.Value > now)
                    return true;
                
                return false;
            }
        }

        // Navigation Properties
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
