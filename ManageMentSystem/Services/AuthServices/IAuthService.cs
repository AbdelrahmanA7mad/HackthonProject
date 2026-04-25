using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ManageMentSystem.Services.AuthServices
{
	public interface IAuthService
	{
		Task<SignInResult> LoginAsync(LoginViewModel model);
		Task<(IdentityResult result, ApplicationUser? user)> RegisterAsync(RegisterViewModel model);
		Task LogoutAsync();
	}
}


