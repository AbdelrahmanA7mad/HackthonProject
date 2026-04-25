using ManageMentSystem.Models;
using ManageMentSystem.Services.HomeServices;
using ManageMentSystem.Services.StoreAccountServices;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.Data;
using ManageMentSystem.ViewModels;
using ManageMentSystem.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _ihomeService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IStoreAccountService _storeAccountService;
        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public HomeController(
            IHomeService homeService, 
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager,
            IStoreAccountService storeAccountService,
            AppDbContext context,
            IUserService userService)
        {
            _ihomeService = homeService;
            _signInManager = signInManager;
            _storeAccountService = storeAccountService;
            _context = context;
            _userService = userService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var model = await _ihomeService.GetDashboardDataAsync();

            // التحقق من الحاجة لإعداد أولي
            // الشرط: الرصيد = 0 AND عدد المعاملات = 0
            var balance = await _storeAccountService.GetTotalCapitalAsync();
            var rootUserId = await _userService.GetRootUserIdAsync();
            var transactionCount = await _context.StoreAccounts
                .Where(s => s.TenantId == rootUserId)
                .CountAsync();
            
            ViewBag.NeedsInitialSetup = (balance == 0 && transactionCount == 0);

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddInitialBalance([FromBody] InitialBalanceRequest request)
        {
            try
            {
                if (request.Amount <= 0)
                {
                    return Json(new { success = false, message = "يرجى إدخال مبلغ صحيح أكبر من صفر" });
                }

                // إنشاء معاملة رصيد افتتاحي
                var transaction = new StoreAccountViewModel
                {
                    TransactionName = "رصيد افتتاحي",
                    TransactionType = TransactionType.Income,
                    Amount = request.Amount,
                    TransactionDate = DateTime.Now,
                    Description = "رصيد وهمي لإضافة المنتجات الموجودة",
                    Category = "رأس مال",
                    PaymentMethodId = await _context.PaymentMethodOptions
                        .Where(pm => pm.IsDefault)
                        .OrderBy(pm => pm.SortOrder)
                        .Select(pm => pm.Id)
                        .FirstOrDefaultAsync(),
                    ReferenceNumber = $"INITIAL-{DateTime.Now:yyyyMMddHHmmss}",
                    Notes = "رصيد افتتاحي لإضافة المنتجات"
                };

                await _storeAccountService.CreateTransactionAsync(transaction);

                return Json(new { success = true, message = "تم إضافة الرصيد بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"حدث خطأ: {ex.Message}" });
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Landing()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Brand = "SalesFlow"; // عدّل الاسم حسب رغبتك
            ViewBag.ContactEmail = "info@salesflow.com"; // ايميل التواصل
            ViewBag.ContactPhone = "+966 50 123 4567"; // هاتف التواصل
            ViewBag.ContactCity = "الرياض، المملكة العربية السعودية"; // العنوان
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Login(LoginViewModel model)
        {
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Logout", "Auth");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return RedirectToAction("Register", "Auth");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Register(RegisterViewModel model)
        {
            return RedirectToAction("Register", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SubscriptionExpired()
        {
             // We need to pass the tenant to the view so the Partial can render
             // Use dependency injection service or context directly? 
             // Accessing User's tenant ID from claims
             var tenantId = User.FindFirst("TenantId")?.Value;
             if (string.IsNullOrEmpty(tenantId)) return RedirectToAction("Index", "Home"); 
             
             // Get Tenant (using IHomeService or context? Service is cleaner but Context is already in DI container, 
             // but here we don't have Context injected.
             // Let's use IHomeService if appropriate? No, HomeService is for Dashboard.
             // We can use the view model or just rely on the Filter to have passed it? 
             // Filter redirects here.
             
             // Quick way: we need DbContext or Service.
             // I'll add IUserService here or just use HttpContext.RequestServices (Anti-pattern but quick).
             // Better: Inject IUserService or DBContext. 
             // HomeController already has SignIn/Manager.
             // Let's rely on ViewBag or fetching it.
             
             // Actually, the _SubscriptionExpiredBanner needs a Tenant Model.
             // I will hack it slightly by getting it from HttpContext.Items if filter puts it there?
             // Or just re-query. Re-query is safest.
             
             var context = HttpContext.RequestServices.GetService<ManageMentSystem.Data.AppDbContext>();
             var tenant = await context.Tenants.FindAsync(tenantId);
             
             return View(tenant);
         }

    }

    // Request model for initial balance submission
    public class InitialBalanceRequest
    {
        public decimal Amount { get; set; }
    }
}