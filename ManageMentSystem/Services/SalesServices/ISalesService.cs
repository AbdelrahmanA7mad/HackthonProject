using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;

namespace ManageMentSystem.Services.SalesServices
{
    public interface ISalesService
    {
        Task<List<Sale>> GetAllSalesAsync();
        Task<(List<Sale> Sales, int TotalCount, int TotalPages)> GetSalesPaginatedAsync(int page, int pageSize, string searchTerm, ManageMentSystem.Models.SalePaymentType? paymentType, DateTime? Date);
        Task<Sale> GetSaleByIdAsync(int id);
        Task<Sale> AddSaleAsync(CreateSaleViewModel model, string currentUserId);
        Task<Sale> UpdateSaleAsync(int id, CreateSaleViewModel model);
        Task<bool> DeleteSaleAsync(int id);
        Task<int> GetTotalAccountProduct(string id, DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalProduct(string id, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<Customer>> GetAllCustomersAsync();
        Task<List<Product>> GetAllProductsAsync();
        Task<Customer> AddCustomerAsync(Customer customer);
        Task<List<PaymentMethodOption>> GetPaymentMethodsAsync();

        Task<(List<Sale> Sales, int TotalCount, int TotalPages, decimal TotalUnpaidAmount)> GetUnpaidSalesAsync(DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20);

    }
} 
