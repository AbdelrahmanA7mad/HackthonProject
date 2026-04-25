using ManageMentSystem.Models;

namespace ManageMentSystem.Services.UserInvoice
{
    public interface IUserInvoice
    {
        Task<Invoice?> GetInvoiceAsync();
        Task EditInvoiceAsync(Invoice invoice);
    }
}
