using ManageMentSystem.Data;
using ManageMentSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ManageMentSystem.ViewComponents
{
    public class SubscriptionStatusViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public SubscriptionStatusViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
                return Content(string.Empty);

            // Get user with tenant
            var user = await _context.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Tenant == null)
                return Content(string.Empty);

            // Return the subscription status
            return View(user.Tenant);
        }
    }
}
