using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;

namespace ManageMentSystem.Services.CustomerAccountServices
{
    public interface ICustomerAccountService
    {
        Task<CustomerPayment> AddPaymentAsync(CustomerPaymentInputViewModel model);
        Task<bool> DeletePaymentAsync(int paymentId);
        Task<CustomerStatementViewModel> GetCustomerStatementAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20);
        Task<decimal> GetCustomerBalanceAsync(int customerId);
        Task<CustomerFullAccountViewModel> GetCustomerFullAccountAsync(int customerId, int page = 1, int pageSize = 20);
    }
}


