using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.PaymentOptionServices;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ManageMentSystem.Services.AuthServices
{
    public class AuthService : IAuthService
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context; // Direct DB access for Tenant creation
        private readonly IPaymentOptionService _paymentOptionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

		public AuthService(
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager, 
            AppDbContext context,
            IPaymentOptionService paymentOptionService,
            IHttpContextAccessor httpContextAccessor)
		{
			_signInManager = signInManager;
			_userManager = userManager;
            _context = context; 
            _paymentOptionService = paymentOptionService;
            _httpContextAccessor = httpContextAccessor;
		}

		public async Task<SignInResult> LoginAsync(LoginViewModel model)
		{
            var input = model.Email?.Trim();
            
            // 1. Try to login as Owner (ApplicationUser) first
            ApplicationUser? user = null;
            if (!string.IsNullOrEmpty(input))
            {
                if (input.Contains("@"))
                    user = await _userManager.FindByEmailAsync(input);
                
                if (user == null)
                    user = await _userManager.FindByNameAsync(input);
            }
            
            if (user != null)
            {
                // Owner login
                if (string.IsNullOrEmpty(user.TenantId))
                {
                    return SignInResult.Failed;
                }

                var tenant = await _context.Tenants.FindAsync(user.TenantId);
                if (tenant == null || !tenant.IsActive)
                {
                    return SignInResult.LockedOut;
                }

                var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (signInResult.Succeeded)
                {
                    user.LastLoginAt = DateTime.Now;
                    await _userManager.UpdateAsync(user);
                }
                return signInResult;
            }

            return SignInResult.Failed;
		}



        public async Task<(IdentityResult result, ApplicationUser? user)> RegisterAsync(RegisterViewModel model)
		{
            // Transaction? Yes, we should use transaction for Tenant + User + Roles
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
			    var existingEmail = await _userManager.FindByEmailAsync(model.Email);
			    if (existingEmail != null) return (IdentityResult.Failed(new IdentityError { Description = "البريد الإلكتروني مسجل بالفعل" }), null);

                var existingUserName = await _userManager.FindByNameAsync(model.Username);
                if (existingUserName != null) return (IdentityResult.Failed(new IdentityError { Description = "اسم المستخدم مسجل بالفعل" }), null);



                // 1. Create Tenant (The Business)
                var tenant = new Tenant
                {
                    Name = !string.IsNullOrWhiteSpace(model.StoreName) ? model.StoreName : (model.FullName + " Store"), // Use StoreName if provided, otherwise default
                    CurrencyCode = model.CurrencyCode ?? "EGP",
                    Phone = model.PhoneNumber, // نقل رقم الهاتف للمؤسسة
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    // Set trial period (30 days from now)
                    TrialEndDate = DateTime.Now.AddDays(10),
                    SubscriptionStatus = "Trial"
                };
                _context.Tenants.Add(tenant);
                await _context.SaveChangesAsync(); 

			    // 2. Create Admin User linked to Tenant
                var user = new ApplicationUser
			    {
				    UserName = model.Username,
				    Email = model.Email,
				    FullName = model.FullName,
				    PhoneNumber = model.PhoneNumber,
				    PreferredCurrency = model.CurrencyCode,
				    EmailConfirmed = true,
				    IsActive = true,
                    TenantId = tenant.Id // Link to new Tenant
			    };

                var createResult = await _userManager.CreateAsync(user, model.Password);
			    if (!createResult.Succeeded)
			    {
                    // Ef Core interaction will be rolled back by transaction if we throw or not commit? 
                    // Manual rollback is safer or just depend on Dispose if not Commited.
                    await transaction.RollbackAsync();
				    return (createResult, null);
			    }


                
                // 4. Initialize Default Payment Options for Tenant
                await InitializePaymentOptionsForTenantAsync(tenant.Id, user.Id);

                await transaction.CommitAsync();
			    return (IdentityResult.Success, user);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (IdentityResult.Failed(new IdentityError { Description = "An error occurred: " + ex.Message }), null);
            }
		}

		public async Task LogoutAsync()
		{
			await _signInManager.SignOutAsync();
			// Also sign out employee if logged in (using Identity scheme)
			await _httpContextAccessor.HttpContext!.SignOutAsync(IdentityConstants.ApplicationScheme);
		}


        // Helper to initialize payment options for a tenant
        private async Task InitializePaymentOptionsForTenantAsync(string tenantId, string userId)
        {
            // Check if tenant already has payment methods
            var hasPaymentMethods = await _context.PaymentMethodOptions
                .AnyAsync(pm => pm.TenantId == tenantId);

            if (!hasPaymentMethods)
            {
                var defaultOptions = new List<PaymentMethodOption>
                {
                    new PaymentMethodOption
                    {
                        Name = "نقدي",
                        IsActive = true,
                        IsDefault = true,
                        SortOrder = 1,
                        TenantId = tenantId,
                        CreatedAt = DateTime.Now
                    },
                    new PaymentMethodOption
                    {
                        Name = "بطاقة ائتمان",
                        IsActive = true,
                        IsDefault = false,
                        SortOrder = 2,
                        TenantId = tenantId,
                        CreatedAt = DateTime.Now
                    },
                    new PaymentMethodOption
                    {
                        Name = "تحويل بنكي",
                        IsActive = true,
                        IsDefault = false,
                        SortOrder = 3,
                        TenantId = tenantId,
                        CreatedAt = DateTime.Now
                    }
                };

                _context.PaymentMethodOptions.AddRange(defaultOptions);
                await _context.SaveChangesAsync();
            }
        }
	}
}
