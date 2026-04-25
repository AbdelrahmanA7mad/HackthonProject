using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;
using ManageMentSystem.Services.UserServices;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Services.SystemSettings
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public SystemSettingsService(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<InventorySettings> GetInventorySettingsAsync()
        {
            try
            {
                var tenantId = await _userService.GetCurrentTenantIdAsync();
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    return GetDefaultInventorySettings();
                }

                var settings = await _context.InventorySettings
                    .FirstOrDefaultAsync(s => s.TenantId == tenantId);

                return settings ?? GetDefaultInventorySettings();
            }
            catch (Exception)
            {
                return GetDefaultInventorySettings();
            }
        }

        public async Task<bool> UpdateInventorySettingsAsync(InventorySettings settings)
        {
            try
            {
                var tenantId = await _userService.GetCurrentTenantIdAsync();
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    return false;
                }

                var existingSettings = await _context.InventorySettings
                    .FirstOrDefaultAsync(s => s.TenantId == tenantId);

                if (existingSettings != null)
                {
                    // Update existing
                    existingSettings.EnableLowStockAlerts = settings.EnableLowStockAlerts;
                    existingSettings.LowStockThreshold = settings.LowStockThreshold;
                    existingSettings.AlertMessage = settings.AlertMessage;
                    existingSettings.ShowAlertOnThreshold = settings.ShowAlertOnThreshold;
                    existingSettings.ShowDashboardAlert = settings.ShowDashboardAlert;
                    existingSettings.ShowProductsPageAlert = settings.ShowProductsPageAlert;
                    existingSettings.ShowReportsAlert = settings.ShowReportsAlert;
                    existingSettings.AlertColor = settings.AlertColor;
                    existingSettings.AlertIcon = settings.AlertIcon;
                }
                else
                {
                    // Create new
                    settings.TenantId = tenantId;
                    _context.InventorySettings.Add(settings);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ResetToDefaultSettingsAsync()
        {
            try
            {
                var tenantId = await _userService.GetCurrentTenantIdAsync();
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    return false;
                }

                var existingSettings = await _context.InventorySettings
                    .FirstOrDefaultAsync(s => s.TenantId == tenantId);

                if (existingSettings != null)
                {
                    _context.InventorySettings.Remove(existingSettings);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private InventorySettings GetDefaultInventorySettings()
        {
            return new InventorySettings
            {
                EnableLowStockAlerts = true,
                LowStockThreshold = 10,
                AlertMessage = "تنبيه: المنتج {0} وصل إلى الحد الأدنى للمخزون ({1} وحدة)",
                ShowAlertOnThreshold = true,
                ShowDashboardAlert = true,
                ShowProductsPageAlert = true,
                ShowReportsAlert = true,
                AlertColor = "warning",
                AlertIcon = "fas fa-exclamation-triangle"
            };
        }

        // Tab Visibility Settings Methods
        public async Task<TabVisibilitySettings> GetTabVisibilitySettingsAsync()
        {
            try
            {
                var tenantId = await _userService.GetCurrentTenantIdAsync();
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    return GetDefaultTabVisibilitySettings();
                }

                var settings = await _context.TabVisibilitySettings
                    .FirstOrDefaultAsync(s => s.TenantId == tenantId);

                return settings ?? GetDefaultTabVisibilitySettings();
            }
            catch (Exception)
            {
                return GetDefaultTabVisibilitySettings();
            }
        }

        public async Task<bool> UpdateTabVisibilitySettingsAsync(TabVisibilitySettings settings)
        {
            try
            {
                var tenantId = await _userService.GetCurrentTenantIdAsync();
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    return false;
                }

                var existingSettings = await _context.TabVisibilitySettings
                    .FirstOrDefaultAsync(s => s.TenantId == tenantId);

                if (existingSettings != null)
                {
                    // Update existing
                    existingSettings.ShowProducts = settings.ShowProducts;
                    existingSettings.ShowCategories = settings.ShowCategories;
                    existingSettings.ShowCustomers = settings.ShowCustomers;
                    existingSettings.ShowSales = settings.ShowSales;
                    existingSettings.ShowInstallments = settings.ShowInstallments;
                    existingSettings.ShowGeneralDebts = settings.ShowGeneralDebts;
                    existingSettings.ShowStoreAccount = settings.ShowStoreAccount;
                    existingSettings.ShowReports = settings.ShowReports;
                }
                else
                {
                    // Create new
                    settings.TenantId = tenantId;
                    _context.TabVisibilitySettings.Add(settings);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private TabVisibilitySettings GetDefaultTabVisibilitySettings()
        {
            return new TabVisibilitySettings
            {
                ShowProducts = true,
                ShowCategories = true,
                ShowCustomers = true,
                ShowSales = true,
                ShowInstallments = true,
                ShowGeneralDebts = true,
                ShowStoreAccount = true,
                ShowReports = true
            };
        }

        // Tenant Settings Implementation
        public async Task<TenantSettingsViewModel> GetTenantSettingsAsync()
        {
            try
            {
                var tenantId = await _userService.GetCurrentTenantIdAsync();
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    return new TenantSettingsViewModel();
                }

                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
                
                if (tenant == null)
                {
                     return new TenantSettingsViewModel();
                }

                return new TenantSettingsViewModel
                {
                    Name = tenant.Name,
                    Address = tenant.Address,
                    Phone = tenant.Phone,
                    LogoUrl = tenant.LogoUrl,
                    CurrencyCode = tenant.CurrencyCode,
                    CreatedAt = tenant.CreatedAt,
                    IsActive = tenant.IsActive,
                    SubscriptionStatus = tenant.SubscriptionStatus,
                    TrialEndDate = tenant.TrialEndDate,
                    SubscriptionEndDate = tenant.SubscriptionEndDate
                };
            }
            catch (Exception)
            {
                return new TenantSettingsViewModel();
            }
        }

        public async Task<bool> UpdateTenantSettingsAsync(TenantSettingsViewModel settings)
        {
            try
            {
                var tenantId = await _userService.GetCurrentTenantIdAsync();
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    return false;
                }

                var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);

                if (tenant != null)
                {
                    // Update editable fields
                    tenant.Name = settings.Name;
                    tenant.Address = settings.Address;
                    tenant.Phone = settings.Phone;
                    tenant.LogoUrl = settings.LogoUrl;
                    tenant.CurrencyCode = settings.CurrencyCode;
                    
                    await _context.SaveChangesAsync();
                    return true;
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
