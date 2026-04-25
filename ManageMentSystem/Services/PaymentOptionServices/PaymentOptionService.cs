using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.UserServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Services.PaymentOptionServices
{
    public class PaymentOptionService : IPaymentOptionService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public PaymentOptionService(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<List<PaymentMethodOption>> GetAllAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            return await _context.PaymentMethodOptions
                .Where(pm => pm.TenantId == tenantId || pm.TenantId == null)
                .OrderBy(pm => pm.SortOrder)
                .ToListAsync();
        }

        public async Task<List<PaymentMethodOption>> GetActiveAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            return await _context.PaymentMethodOptions
                .Where(pm => pm.IsActive && (pm.TenantId == tenantId || pm.TenantId == null))
                .OrderBy(pm => pm.SortOrder)
                .ToListAsync();
        }

        public async Task<SelectList> GetSelectListAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var methods = await _context.PaymentMethodOptions
                .Where(pm => pm.IsActive && (pm.TenantId == tenantId || pm.TenantId == null))
                .OrderBy(pm => pm.SortOrder)
                .Select(pm => new { Value = pm.Id, Text = pm.Name })
                .ToListAsync();

            return new SelectList(methods, "Value", "Text");
        }

        public async Task<PaymentMethodOption?> GetByIdAsync(int id)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            return await _context.PaymentMethodOptions
                .FirstOrDefaultAsync(pm => pm.Id == id && (pm.TenantId == tenantId || pm.TenantId == null));
        }

        public async Task<PaymentMethodOption?> GetDefaultAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            return await _context.PaymentMethodOptions
                .Where(pm => pm.IsDefault && (pm.TenantId == tenantId || pm.TenantId == null))
                .OrderBy(pm => pm.SortOrder)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetDefaultIdAsync()
        {
            var defaultMethod = await GetDefaultAsync();
            return defaultMethod?.Id;
        }

        public async Task<PaymentMethodOption> CreateAsync(PaymentMethodOption paymentMethod)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");

            paymentMethod.TenantId = tenantId;
            paymentMethod.CreatedAt = DateTime.Now;
            
            _context.PaymentMethodOptions.Add(paymentMethod);
            await _context.SaveChangesAsync();
            
            return paymentMethod;
        }

        public async Task UpdateAsync(PaymentMethodOption paymentMethod)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var existingMethod = await _context.PaymentMethodOptions
                .FirstOrDefaultAsync(pm => pm.Id == paymentMethod.Id && (pm.TenantId == tenantId || pm.TenantId == null));

            if (existingMethod == null)
                throw new InvalidOperationException("طريقة الدفع غير موجودة أو لا تملك صلاحية تعديلها");

            existingMethod.Name = paymentMethod.Name;
            existingMethod.IsActive = paymentMethod.IsActive;
            existingMethod.IsDefault = paymentMethod.IsDefault;
            existingMethod.SortOrder = paymentMethod.SortOrder;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var paymentMethod = await _context.PaymentMethodOptions
                .FirstOrDefaultAsync(pm => pm.Id == id && (pm.TenantId == tenantId || pm.TenantId == null));

            if (paymentMethod == null)
                throw new InvalidOperationException("طريقة الدفع غير موجودة أو لا تملك صلاحية حذفها");

            _context.PaymentMethodOptions.Remove(paymentMethod);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            return await _context.PaymentMethodOptions
                .AnyAsync(pm => pm.Id == id && (pm.TenantId == tenantId || pm.TenantId == null));
        }

        public async Task<List<PaymentMethodOption>> GetByUserIdAsync(string userId)
        {
            // للحفاظ على التوافق مع الكود القديم، نستخدم TenantId
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            return await _context.PaymentMethodOptions
                .Where(pm => pm.TenantId == tenantId || pm.TenantId == null)
                .OrderBy(pm => pm.SortOrder)
                .ToListAsync();
        }

        public async Task<SelectList> GetSelectListByUserIdAsync(string userId)
        {
            // للحفاظ على التوافق مع الكود القديم، نستخدم TenantId
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var methods = await _context.PaymentMethodOptions
                .Where(pm => pm.IsActive && (pm.TenantId == tenantId || pm.TenantId == null))
                .OrderBy(pm => pm.SortOrder)
                .Select(pm => new { Value = pm.Id, Text = pm.Name })
                .ToListAsync();

            return new SelectList(methods, "Value", "Text");
        }

        public async Task InitializeDefaultOptionsAsync(string userId)
        {
            // للحفاظ على التوافق مع الكود القديم، نستخدم TenantId
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            // تحقق مما إذا كان المستأجر لديه بالفعل طرق دفع
            var hasPaymentMethods = await _context.PaymentMethodOptions
                .AnyAsync(pm => pm.TenantId == tenantId || pm.TenantId == null);

            if (!hasPaymentMethods)
            {
                var userIdForCreate = await _userService.GetCurrentUserIdAsync();
                var defaultOptions = new List<PaymentMethodOption>
                {
                    new PaymentMethodOption
                    {
                        Name = "نقدي",
                        IsActive = true,
                        IsDefault = true,
                        SortOrder = 1,
                        TenantId = tenantId,
                        CreatedAt = DateTime.Now
                    },
                    new PaymentMethodOption
                    {
                        Name = "بطاقة ائتمان",
                        IsActive = true,
                        IsDefault = false,
                        SortOrder = 2,
                        TenantId = tenantId,
                        CreatedAt = DateTime.Now
                    },
                    new PaymentMethodOption
                    {
                        Name = "تحويل بنكي",
                        IsActive = true,
                        IsDefault = false,
                        SortOrder = 3,
                        TenantId = tenantId,
                        CreatedAt = DateTime.Now
                    }
                };

                _context.PaymentMethodOptions.AddRange(defaultOptions);
                await _context.SaveChangesAsync();
            }
        }
    }
}
