using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;

namespace ManageMentSystem.Services.InstallmentServices
{
    public interface IInstallmentService
    {
        Task<List<Installment>> GetAllInstallmentsAsync();
        Task<Installment> GetInstallmentByIdAsync(int id);
        Task<Installment> AddInstallmentAsync(CreateInstallmentViewModel model);
        Task<bool> DeleteInstallmentAsync(int id);
        Task<bool> RescheduleInstallmentAsync(int id, int newMonths);
        Task<List<Installment>> GetInstallmentsWithDetailsAsync();
        Task<List<Customer>> GetAllCustomersAsync();
        Task<List<Product>> GetAllProductsAsync();
        Task<Customer> AddCustomerAsync(Customer customer);
        Task<InstallmentDetailsViewModel> GetInstallmentDetailsWithMonthlyPaymentsAsync(int id);
        Task UpdateInstallmentRemainingAmountAsync(int installmentId);
        
        // Extra month management methods
        Task<bool> AddExtraMonthAsync(int installmentId);
        Task<bool> RemoveExtraMonthAsync(int installmentId);
        Task<decimal> CalculateExtraMonthAmountAsync(int installmentId);

        // Summary statistics method
        Task<InstallmentSummaryViewModel> GetInstallmentSummaryAsync();
        
        // Filtered methods
        Task<List<Installment>> GetFilteredInstallmentsAsync(DateTime startDate, DateTime endDate, string filterType);
        Task<InstallmentSummaryViewModel> GetFilteredInstallmentSummaryAsync(DateTime startDate, DateTime endDate, string filterType);

        // Pagination methods
        Task<PaginatedInstallmentsViewModel> GetPaginatedInstallmentsAsync(int pageNumber, int pageSize, string searchTerm = "", string sortBy = "StartDate", string sortOrder = "desc");
        Task UpdateOverdueInstallmentsAsync();
        
        // Overdue installments method
        Task<List<Installment>> GetOverdueInstallmentsAsync();

    }
} 
