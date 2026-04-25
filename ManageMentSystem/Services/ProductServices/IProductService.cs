using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;

namespace ManageMentSystem.Services.ProductServices
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<(bool Success, string Message, Product? Product)> AddProductWithBalanceCheckAsync(CreateProductViewModel model);
        Task<(bool Success, string Message, Product? Product)> UpdateProductAsync(int id, CreateProductViewModel model);
        Task<(bool Success, string Message)> DeleteProductAsync(int id);
        Task<List<Product>> GetLowStockProductsAsync(int threshold = 2);
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<bool> IsBarcodeUniqueAsync(string barcode, int? excludeProductId = null);
        Task<Product?> GetProductByBarcodeAsync(string barcode);
    }
}
