using ManageMentSystem.Services.AuthServices;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageMentSystem.Controllers
{
	public class AuthController : Controller
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult Login()
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}
			return View(new LoginViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var result = await _authService.LoginAsync(model);
			if (result.Succeeded)
			{
				return RedirectToAction("Index", "Home");
			}

			ModelState.AddModelError(string.Empty, "اسم المستخدم أو كلمة المرور غير صحيحة");
			return View(model);
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult Register()
		{
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}
			return View(new RegisterViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var (result, user) = await _authService.RegisterAsync(model);
			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
				return View(model);
			}

			return RedirectToAction("Login");
		}

		public async Task<IActionResult> Logout()
		{
			await _authService.LogoutAsync();
			return RedirectToAction("Login");
		}
	}
}


