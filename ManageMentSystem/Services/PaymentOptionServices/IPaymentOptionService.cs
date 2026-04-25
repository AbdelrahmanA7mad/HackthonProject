using ManageMentSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageMentSystem.Services.PaymentOptionServices
{
    public interface IPaymentOptionService
    {
        Task<List<PaymentMethodOption>> GetAllAsync();
        Task<List<PaymentMethodOption>> GetActiveAsync();
        Task<SelectList> GetSelectListAsync();
        Task<PaymentMethodOption?> GetByIdAsync(int id);
        Task<PaymentMethodOption?> GetDefaultAsync();
        Task<int?> GetDefaultIdAsync();
        Task<PaymentMethodOption> CreateAsync(PaymentMethodOption paymentMethod);
        Task UpdateAsync(PaymentMethodOption paymentMethod);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<PaymentMethodOption>> GetByUserIdAsync(string userId);
        Task<SelectList> GetSelectListByUserIdAsync(string userId);
        Task InitializeDefaultOptionsAsync(string userId);
    }
}
