using ManageMentSystem.Helpers;
using ManageMentSystem.Models;
using ManageMentSystem.Services.SystemSettings;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class SystemSettingsController : Controller
    {
        private readonly ISystemSettingsService _systemSettingsService;
        private readonly ManageMentSystem.Data.AppDbContext _context;

        public SystemSettingsController(ISystemSettingsService systemSettingsService, ManageMentSystem.Data.AppDbContext context)
        {
            _systemSettingsService = systemSettingsService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        // GET: SystemSettings/Inventory
                public async Task<IActionResult> Inventory()
        {
            var settings = await _systemSettingsService.GetInventorySettingsAsync();
            return View(settings);
        }

        // POST: SystemSettings/Inventory
        [HttpPost]
        [ValidateAntiForgeryToken]
                public async Task<IActionResult> Inventory(InventorySettings model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _systemSettingsService.UpdateInventorySettingsAsync(model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "ØªÙ… ØªØ­Ø¯ÙŠØ« Ø¥Ø¹Ø¯Ø§Ø¯Ø§Øª Ø§Ù„Ù…Ø®Ø²ÙˆÙ† Ø¨Ù†Ø¬Ø§Ø­!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Ø­Ø¯Ø« Ø®Ø·Ø£ Ø£Ø«Ù†Ø§Ø¡ ØªØ­Ø¯ÙŠØ« Ø§Ù„Ø¥Ø¹Ø¯Ø§Ø¯Ø§Øª.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ø­Ø¯Ø« Ø®Ø·Ø£ ØºÙŠØ± Ù…ØªÙˆÙ‚Ø¹: {ex.Message}";
            }

            return View(model);
        }

        // GET: SystemSettings/TabVisibility
                public async Task<IActionResult> TabVisibility()
        {
            var settings = await _systemSettingsService.GetTabVisibilitySettingsAsync();
            return View(settings);
        }

        // POST: SystemSettings/TabVisibility
        [HttpPost]
        [ValidateAntiForgeryToken]
                public async Task<IActionResult> TabVisibility(TabVisibilitySettings model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _systemSettingsService.UpdateTabVisibilitySettingsAsync(model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "ØªÙ… ØªØ­Ø¯ÙŠØ« Ø¥Ø¹Ø¯Ø§Ø¯Ø§Øª Ø¸Ù‡ÙˆØ± Ø§Ù„Ù‚ÙˆØ§Ø¦Ù… Ø¨Ù†Ø¬Ø§Ø­!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Ø­Ø¯Ø« Ø®Ø·Ø£ Ø£Ø«Ù†Ø§Ø¡ ØªØ­Ø¯ÙŠØ« Ø§Ù„Ø¥Ø¹Ø¯Ø§Ø¯Ø§Øª.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ø­Ø¯Ø« Ø®Ø·Ø£ ØºÙŠØ± Ù…ØªÙˆÙ‚Ø¹: {ex.Message}";
            }

            return View(model);
        }

        // GET: SystemSettings/TenantSettings
                public async Task<IActionResult> TenantSettings()
        {
            var settings = await _systemSettingsService.GetTenantSettingsAsync();
            return View(settings);
        }

        // POST: SystemSettings/TenantSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
                public async Task<IActionResult> TenantSettings(ViewModels.TenantSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _systemSettingsService.UpdateTenantSettingsAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = "ØªÙ… ØªØ­Ø¯ÙŠØ« Ø¥Ø¹Ø¯Ø§Ø¯Ø§Øª Ø§Ù„Ù…Ø¤Ø³Ø³Ø© Ø¨Ù†Ø¬Ø§Ø­!";
                    // Force a page reload on client side to update currency symbol if needed via TempData flag, 
                    // though Layout usually handles it if we refresh. 
                    // Let's rely on standard redirect.
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Ø­Ø¯Ø« Ø®Ø·Ø£ Ø£Ø«Ù†Ø§Ø¡ ØªØ­Ø¯ÙŠØ« Ø§Ù„Ø¥Ø¹Ø¯Ø§Ø¯Ø§Øª.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ø­Ø¯Ø« Ø®Ø·Ø£ ØºÙŠØ± Ù…ØªÙˆÙ‚Ø¹: {ex.Message}";
            }

            return View(model);
        }
    
        // GET: SystemSettings/Subscription
        public async Task<IActionResult> Subscription()
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId)) return RedirectToAction("Index", "Home");

            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null) return NotFound();

            return View(tenant);
        }

    }

    }


