using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;

namespace ManageMentSystem.Services.InstallmentServices
{
    public interface IInstallmentPaymentService
    {
        Task<IEnumerable<InstallmentPayment>> GetAllPaymentsAsync();
        Task<IEnumerable<InstallmentPayment>> GetPaymentsByInstallmentIdAsync(int installmentId);
        Task<InstallmentPayment> GetPaymentByIdAsync(int id);
        Task<InstallmentPayment> AddPaymentAsync(CreateInstallmentPaymentViewModel model);
        Task<InstallmentPayment> UpdatePaymentAsync(int id, CreateInstallmentPaymentViewModel model);
        Task<bool> DeletePaymentAsync(int id);

        Task<decimal> GetTotalPaidForInstallmentAsync(int installmentId);
        Task<int> GetPaidMonthsForInstallmentAsync(int installmentId);
        Task<CreateInstallmentPaymentViewModel> GetInstallmentDetailsForPaymentAsync(int installmentId);
    }
} 
