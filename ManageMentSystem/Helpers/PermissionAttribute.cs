using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManageMentSystem.Helpers
{
	/// <summary>
	/// Authorization attribute - now simplified: just requires authenticated user.
	/// Previously checked specific permissions; now all authenticated users have full access.
	/// Kept for backward compatibility with existing usages like [Permission(Permission.ViewDashboard)].
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class PermissionAttribute : TypeFilterAttribute
	{
		public PermissionAttribute(params object[] args) : base(typeof(PermissionFilter))
		{
		}
	}

	public class PermissionFilter : IAsyncAuthorizationFilter
	{
		public Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			// Check if user is authenticated - all authenticated users have full access
			if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
			{
				context.Result = new RedirectToActionResult("Login", "Auth", null);
			}
			return Task.CompletedTask;
		}
	}

	/// <summary>
	/// Authorization attribute that requires user to be an Owner.
	/// Now simplified: any authenticated user is considered an owner.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class RequireOwnerAttribute : TypeFilterAttribute
	{
		public RequireOwnerAttribute() : base(typeof(RequireOwnerFilter))
		{
		}
	}

	public class RequireOwnerFilter : IAsyncAuthorizationFilter
	{
		public Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			// Any authenticated user is now treated as owner
			if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
			{
				context.Result = new RedirectToActionResult("Login", "Auth", null);
			}
			return Task.CompletedTask;
		}
	}
}
