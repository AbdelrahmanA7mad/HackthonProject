using ManageMentSystem.Data;
using ManageMentSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ManageMentSystem.Services.UserServices
{
	public class UserService : IUserService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly AppDbContext _context;

		public UserService(
			IHttpContextAccessor httpContextAccessor,
			UserManager<ApplicationUser> userManager,
			AppDbContext context)
		{
			_httpContextAccessor = httpContextAccessor;
			_userManager = userManager;
			_context = context;
		}

		public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

		public string GetUserId()
		{
			return User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
		}

		public bool IsAuthenticated()
		{
			return User?.Identity?.IsAuthenticated == true;
		}

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return null;
            
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<string?> GetCurrentTenantIdAsync()
        {
            // 1. Try to get from Claims (Fastest) - added on login via CustomUserClaimsPrincipalFactory
            var tenantIdClaim = User?.FindFirstValue("TenantId");
            if (!string.IsNullOrEmpty(tenantIdClaim)) return tenantIdClaim;

            // 2. Fallback to Database
            var user = await GetCurrentUserAsync();
            return user?.TenantId;
        }

        public async Task<Tenant?> GetCurrentTenantAsync()
        {
            var tenantId = await GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return null;

            return await _context.Tenants.FindAsync(tenantId);
        }



        public async Task<string> GetRootUserIdAsync()
        {
            var tenantId = await GetCurrentTenantIdAsync();
            return tenantId ?? string.Empty;
        }

        public async Task<string> GetCurrentUserIdAsync()
        {
            return GetUserId();
        }
	}
}
