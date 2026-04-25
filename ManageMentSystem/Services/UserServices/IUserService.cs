using ManageMentSystem.Models;
using System.Security.Claims;

namespace ManageMentSystem.Services.UserServices
{
	public interface IUserService
	{
		ClaimsPrincipal? User { get; }
		string GetUserId();
		bool IsAuthenticated();
		Task<ApplicationUser?> GetCurrentUserAsync();
        
        // Tenant support
        Task<string?> GetCurrentTenantIdAsync();
        Task<Tenant?> GetCurrentTenantAsync();


        
        // Compatibility method: Returns TenantId
        Task<string> GetRootUserIdAsync();
        
        // Get current user ID (ApplicationUser.Id)
        Task<string> GetCurrentUserIdAsync();
	}
}
