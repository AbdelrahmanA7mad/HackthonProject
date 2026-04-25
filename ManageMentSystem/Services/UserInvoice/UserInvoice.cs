using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.UserServices;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Services.UserInvoice
{
    public class UserInvoice : IUserInvoice
    {
        private readonly IUserService _user;
        private readonly AppDbContext _context;

        public UserInvoice(IUserService user, AppDbContext context)
        {
            _user = user;
            _context = context;
        }

        public async Task<Invoice?> GetInvoiceAsync()
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.TenantId == tenantId);
            return invoice;
        }

        public async Task EditInvoiceAsync(Invoice invoice)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();

            // البحث عن الفاتورة الموجودة
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.TenantId == tenantId);

            if (existingInvoice != null)
            {
                // تحديث البيانات الموجودة
                existingInvoice.CompanyName = invoice.CompanyName;
                existingInvoice.CompanySubtitle = invoice.CompanySubtitle;
                existingInvoice.Address = invoice.Address;
                existingInvoice.FooterMessage = invoice.FooterMessage;
                existingInvoice.Logo = invoice.Logo;
                existingInvoice.Website = invoice.Website;
                existingInvoice.Email = invoice.Email;
                existingInvoice.PhoneNumbers = invoice.PhoneNumbers ?? new List<string>();

                _context.Invoices.Update(existingInvoice);
            }
            else
            {
                // لا توجد فاتورة موجودة لذا نقوم بإضافة جديدة
                invoice.TenantId = tenantId;
                invoice.UserId = await _user.GetCurrentUserIdAsync();
                _context.Invoices.Add(invoice);
            }

            await _context.SaveChangesAsync();
        }
    }
}
