using DinkToPdf;
using DinkToPdf.Contracts;
using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services;
using ManageMentSystem.Services.CategoryServices;
using ManageMentSystem.Services.CustomerAccountServices;
using ManageMentSystem.Services.CustomerServices;
using ManageMentSystem.Services.GeneralDebtServices;
using ManageMentSystem.Services.HomeServices;
using ManageMentSystem.Services.InstallmentServices;
using ManageMentSystem.Services.PaymentOptionServices;
using ManageMentSystem.Services.ProductServices;
using ManageMentSystem.Services.SalesServices;
using ManageMentSystem.Services.StatisticsServices;
using ManageMentSystem.Services.StoreAccountServices;
using ManageMentSystem.Services.SystemSettings;
using ManageMentSystem.Services.UserInvoice;
using ManageMentSystem.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;
using System.Text.Json.Serialization;
using ManageMentSystem.Services.ExcelExportServices;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
{
    options.EnableEndpointRouting = true;
    options.Filters.Add<ManageMentSystem.Filters.SubscriptionValidationFilter>();
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configure routing options
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = false;
    options.AppendTrailingSlash = false;
});

// Global authorization: require authenticated users by default
// Global authorization: require authenticated users by default
builder.Services.AddAuthorization(options =>
{
	options.FallbackPolicy = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.Build();
});

// Add HttpContextAccessor for accessing HttpContext in services
builder.Services.AddHttpContextAccessor();

// Add Memory Cache for performance optimization
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (Users + Roles) registration
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<ManageMentSystem.Services.ArabicIdentityErrorDescriber>()
    .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(5);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Register AuthService
builder.Services.AddScoped<ManageMentSystem.Services.AuthServices.IAuthService, ManageMentSystem.Services.AuthServices.AuthService>();

// Register services
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<SystemSettingsService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStoreAccountService, StoreAccountService>();
builder.Services.AddScoped<IGeneralDebtService, GeneralDebtService>();
builder.Services.AddScoped<ICustomerAccountService, CustomerAccountService>();
builder.Services.AddScoped<IUserInvoice, UserInvoice>();
builder.Services.AddScoped<IInstallmentService, InstallmentService>();
builder.Services.AddScoped<IInstallmentPaymentService, InstallmentPaymentService>();
builder.Services.AddScoped<IPaymentOptionService, PaymentOptionService>();

// Register System Settings Service
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();

// Register Excel Export Service
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

builder.Services.AddScoped<IUserService, UserService>();

// Register WhatsApp Service
builder.Services.AddScoped<ManageMentSystem.Services.WhatsAppServices.IWhatsAppService, ManageMentSystem.Services.WhatsAppServices.WhatsAppService>();

// ── Gemini AI Services ─────────────────────────────────────────────────────
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["Gemini:ApiKey"]!;
    return new Google.GenAI.Client(apiKey: apiKey);
});
builder.Services.AddScoped<ManageMentSystem.Services.AiServices.IAiToolExecutor,
                           ManageMentSystem.Services.AiServices.AiToolExecutor>();
builder.Services.AddScoped<ManageMentSystem.Services.AiServices.IAiOrchestratorService,
                           ManageMentSystem.Services.AiServices.AiOrchestratorService>();
// ───────────────────────────────────────────────────────────────────────────

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddScoped<PdfService>();

// Session Configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.MaxAge = TimeSpan.FromHours(8);
});

// Cookie Policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

var app = builder.Build();


var arEg = new CultureInfo("ar-EG");
// كثير من المتصفحات ترسل الأرقام العشرية بنقطة وليس فاصلة،
// فنضبط الـ NumberFormat لكي يقبل النقطة بشكل افتراضي لتفادي مشاكل ModelBinding
arEg.NumberFormat.NumberDecimalSeparator = ".";
arEg.NumberFormat.CurrencyDecimalSeparator = ".";
// يمكن تخصيص رمز العملة إذا رغبت
// arEg.NumberFormat.CurrencySymbol = "ج.م";

var supportedCultures = new[] { arEg };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(arEg),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

// كذلك نضبط الثقافة الافتراضية لمؤشرات الخيوط
CultureInfo.DefaultThreadCurrentCulture = arEg;
CultureInfo.DefaultThreadCurrentUICulture = arEg;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization(localizationOptions);
app.UseCookiePolicy(); // أضف دي قبل UseSession

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// Override currency formatting per authenticated user's preferred currency (must run AFTER authentication)
app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        try
        {
            // الوصول لـ UserManager و DbContext
            var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

            var user = await userManager.GetUserAsync(context.User);

            if (user != null)
            {
                // جلب بيانات الـ Tenant بناءً على الـ TenantId الموجود في بيانات المستخدم
                // (تأكد من أن اسم الخاصية في موديل اليوزر هو TenantId واسم الجدول هو Tenants)
                var tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == user.TenantId);

                string preferredCurrency = tenant?.CurrencyCode ?? "EGP"; // افتراضي جنيه مصري إذا لم يوجد

                CultureInfo culture = new CultureInfo("ar-EG");
                culture.NumberFormat.NumberDecimalSeparator = ".";
                culture.NumberFormat.CurrencyDecimalSeparator = ".";

                // التحقق من العملة الموجودة في جدول الـ Tenant
                if (string.Equals(preferredCurrency, "SAR", StringComparison.OrdinalIgnoreCase))
                {
                    culture = new CultureInfo("ar-SA");
                    culture.NumberFormat.NumberDecimalSeparator = ".";
                    culture.NumberFormat.CurrencyDecimalSeparator = ".";
                    // الحفاظ على التقويم الميلادي
                    culture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();
                }
                else if (string.Equals(preferredCurrency, "USD", StringComparison.OrdinalIgnoreCase))
                {
                    culture = new CultureInfo("en-US");
                }

                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                // لضمان استمرارية الإعدادات في نفس الطلب الحالي
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }
        catch
        {
            // يفضل تسجيل الخطأ هنا في حالة وجود مشكلة في قاعدة البيانات
        }
    }

    await next();
}); 
app.UseSession();

// Route for Index actions (must come before default route)
// This allows /Home instead of /Home/Index
app.MapControllerRoute(
    name: "controller-only-index",
    pattern: "{controller}",
    defaults: new { action = "Index" });

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");



app.Run();