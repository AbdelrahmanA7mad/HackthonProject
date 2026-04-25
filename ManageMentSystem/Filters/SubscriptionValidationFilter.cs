using ManageMentSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ManageMentSystem.Filters
{
    public class SubscriptionValidationFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _context;

        public SubscriptionValidationFilter(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            // 1. Skip if not authenticated
            if (user == null || !user.Identity.IsAuthenticated)
            {
                await next();
                return;
            }

            // 2. Skip specific controllers/actions to avoid loops or blocking login/logout
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            // Allow Auth (Login, Logout, Register), Home (Landing, SubscriptionExpired)
            // Also allow Home/Error or Home/AccessDenied if needed
            if (string.Equals(controller, "Auth", StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(controller, "Home", StringComparison.OrdinalIgnoreCase) && 
                 (string.Equals(action, "Landing", StringComparison.OrdinalIgnoreCase) || 
                  string.Equals(action, "SubscriptionExpired", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(action, "Logout", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(action, "Error", StringComparison.OrdinalIgnoreCase))))
            {
                await next();
                return;
            }

            // 3. Check Tenant Subscription
            var tenantId = user.FindFirst("TenantId")?.Value;
            
            // If User is authenticated but has no tenant (e.g. system admin?), allow or skip?
            // In this system, all users (Owner/Employee) have TenantId.
            if (string.IsNullOrEmpty(tenantId))
            {
                await next();
                return;
            }

            // Optimization: We could use a caching layer here to avoid DB hit on every request.
            // For now, simple DB check.
            var tenant = await _context.Tenants.FindAsync(tenantId);

            if (tenant != null && !tenant.IsSubscriptionActive)
            {
                // Subscription Expired!
                
                // If AJAX or API, return 403 or specific JSON
                if (IsAjaxRequest(context.HttpContext.Request))
                {
                    context.Result = new StatusCodeResult(403);
                }
                else
                {
                    // Redirect to SubscriptionExpired page
                    context.Result = new RedirectToActionResult("SubscriptionExpired", "Home", null);
                }
                return;
            }

            await next();
        }

        private bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
