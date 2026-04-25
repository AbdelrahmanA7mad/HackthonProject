using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;

namespace ManageMentSystem.Services.CustomerServices
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllCustomersAsync();
        Task<(List<Customer> Customers, int TotalCount, int TotalPages)> GetCustomersPaginatedAsync(int page = 1, int pageSize = 20, string searchTerm = "");
        Task<Customer> GetCustomerByIdAsync(int id);
        Task<Customer> AddCustomerAsync(CreateCustomerViewModel model);
        Task<Customer> UpdateCustomerAsync(int id, CreateCustomerViewModel model);
        Task<(bool Success, string Message)> DeleteCustomerAsync(int id);
        Task<List<Customer>> GetCustomersWithSalesAsync();
        Task<List<Customer>> GetAllCustomersWithDetailsAsync();
        
        // New methods for phone number validation
        Task<Customer?> GetCustomerByPhoneAsync(string phoneNumber);
        Task<(bool Success, Customer? Customer, string Message)> AddOrUpdateCustomerAsync(CreateCustomerViewModel model);
    }
} 
