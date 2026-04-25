using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;

namespace ManageMentSystem.Services.GeneralDebtServices
{
    public interface IGeneralDebtService
    {
        Task<List<GeneralDebt>> GetAllAsync();
        Task<GeneralDebt?> GetByIdAsync(int id);
        Task<List<PaymentMethodOption>> GetPaymentMethodsAsync();
        Task<(GeneralDebt debt, string? infoMessage)> CreateAsync(CreateGeneralDebtViewModel model);
        Task<GeneralDebt> UpdateAsync(int id, CreateGeneralDebtViewModel model);
        Task DeleteAsync(int id);
        Task<(decimal residual, string? warningMessage)> AddPaymentAsync(int id, decimal amount, int? paymentMethodId, string? description = null);
    }
}
