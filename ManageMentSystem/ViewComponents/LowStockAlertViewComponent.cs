using Microsoft.AspNetCore.Mvc;
using ManageMentSystem.Models;
using ManageMentSystem.Services.ProductServices;
using ManageMentSystem.Services.SystemSettings;

namespace ManageMentSystem.ViewComponents
{
    public class LowStockAlertViewComponent : ViewComponent
    {
        private readonly ISystemSettingsService _systemSettingsService;
        private readonly IProductService _productService;

        public LowStockAlertViewComponent(ISystemSettingsService systemSettingsService, IProductService productService)
        {
            _systemSettingsService = systemSettingsService;
            _productService = productService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var inventorySettings = await _systemSettingsService.GetInventorySettingsAsync();
                
                // إذا لم تكن التنبيهات مفعلة، لا تعرض شيئاً
                if (!inventorySettings.EnableLowStockAlerts)
                {
                    return Content(string.Empty);
                }

                // الحصول على المنتجات التي وصلت للحد الأدنى
                var lowStockProducts = await _productService.GetLowStockProductsAsync(inventorySettings.LowStockThreshold);
                
                if (!lowStockProducts.Any())
                {
                    return Content(string.Empty);
                }

                var viewModel = new LowStockAlertViewModel
                {
                    Products = lowStockProducts,
                    Settings = inventorySettings,
                    AlertMessage = string.Format(inventorySettings.AlertMessage, 
                        lowStockProducts.First().Name, 
                        lowStockProducts.First().Quantity)
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                // في حالة حدوث خطأ، لا تعرض شيئاً
                return Content(string.Empty);
            }
        }
    }

    public class LowStockAlertViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public InventorySettings Settings { get; set; } = new InventorySettings();
        public string AlertMessage { get; set; } = string.Empty;
    }
}
