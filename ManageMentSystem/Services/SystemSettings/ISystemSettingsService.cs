using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;

namespace ManageMentSystem.Services.SystemSettings
{
    public interface ISystemSettingsService
    {
        Task<InventorySettings> GetInventorySettingsAsync();
        Task<bool> UpdateInventorySettingsAsync(InventorySettings settings);
        Task<bool> ResetToDefaultSettingsAsync();
        
        // Tab Visibility Settings
        Task<TabVisibilitySettings> GetTabVisibilitySettingsAsync();
        Task<bool> UpdateTabVisibilitySettingsAsync(TabVisibilitySettings settings);

        // Tenant Settings
        Task<TenantSettingsViewModel> GetTenantSettingsAsync();
        Task<bool> UpdateTenantSettingsAsync(TenantSettingsViewModel settings);
    }
}
