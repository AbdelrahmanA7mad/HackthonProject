using DocumentFormat.OpenXml.Spreadsheet;
using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Services.CustomerServices
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public CustomerService(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var userId = await _userService.GetRootUserIdAsync();
            return await _context.Customers
                .Where(c => c.TenantId == userId)
                .OrderByDescending(c => c.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(List<Customer> Customers, int TotalCount, int TotalPages)> GetCustomersPaginatedAsync(
            int page = 1,
            int pageSize = 20,
            string searchTerm = "")
        {
            page = Math.Max(page, 1);
            pageSize = Math.Max(pageSize, 1);

            var userId = await _userService.GetRootUserIdAsync();

            var query = _context.Customers
                .Where(c => c.TenantId == userId)
                .Include(c => c.Sales)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(searchTerm) ||
                    c.PhoneNumber.Contains(searchTerm) ||
                    (c.Address != null && c.Address.ToLower().Contains(searchTerm))
                );
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var customers = await query
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (customers, totalCount, totalPages);
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            return await _context.Customers
                .Where(c => c.TenantId == tenantId && c.Id == id)
                .Include(c => c.Sales)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<Customer> AddCustomerAsync(CreateCustomerViewModel model)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");

            var customer = new Customer
            {
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                TenantId = tenantId,
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(int id, CreateCustomerViewModel model)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();

            // التحقق من ملكية العميل للمستأجر الحالي
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

            if (customer == null)
                return null;

            customer.FullName = model.FullName;
            customer.PhoneNumber = model.PhoneNumber;
            customer.Address = model.Address;

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<(bool Success, string Message)> DeleteCustomerAsync(int id)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();

            // التحقق من ملكية العميل للمستأجر الحالي
            var customer = await _context.Customers
                .Include(c => c.Sales)
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

            if (customer == null)
                return (false, "العميل غير موجود أو ليس لديك صلاحية للوصول إليه");

            if (customer.Sales != null && customer.Sales.Any())
            {
                return (false, "لا يمكن حذف العميل لأنه لديه عمليات بيع مرتبطة به");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return (true, "تم حذف العميل بنجاح");
        }

        public async Task<List<Customer>> GetCustomersWithSalesAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            return await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .Include(c => c.Sales)
                .OrderByDescending(c => c.Sales.Count)
                .Take(10)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Customer>> GetAllCustomersWithDetailsAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            return await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .Include(c => c.Sales)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByPhoneAsync(string phoneNumber)
        {
            var userId = await _userService.GetRootUserIdAsync();

            // التحقق من ملكية العميل للمستخدم الحالي
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber && c.TenantId == userId);
        }

        public async Task<(bool Success, Customer? Customer, string Message)> AddOrUpdateCustomerAsync(CreateCustomerViewModel model)
        {
            var userId = await _userService.GetRootUserIdAsync();

            // البحث عن العميل بناءً على رقم الهاتف والمستخدم الحالي
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == model.PhoneNumber && c.TenantId == userId);

            if (existingCustomer != null)
            {
                // تحديث بيانات العميل الموجود
                existingCustomer.FullName = model.FullName;
                existingCustomer.Address = model.Address;

                _context.Customers.Update(existingCustomer);
                await _context.SaveChangesAsync();

                return (true, existingCustomer, "تم تحديث بيانات العميل الموجود بنجاح");
            }
            else
            {
                // إضافة عميل جديد
                var tenantId = await _userService.GetCurrentTenantIdAsync();
                var userIdForCreate = await _userService.GetCurrentUserIdAsync();
                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userIdForCreate))
                    throw new InvalidOperationException("المستخدم غير مسجل دخول");

                var customer = new Customer
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    TenantId = tenantId,
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return (true, customer, "تم إضافة العميل الجديد بنجاح");
            }
        }
    }
}
