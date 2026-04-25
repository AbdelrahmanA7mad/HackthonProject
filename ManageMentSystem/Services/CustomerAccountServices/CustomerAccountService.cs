using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.Services.PaymentOptionServices;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Services.CustomerAccountServices
{
    public class CustomerAccountService : ICustomerAccountService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _user;
        private readonly IPaymentOptionService _paymentOptionService;

        public CustomerAccountService(
            AppDbContext context,
            IUserService user,
            IPaymentOptionService paymentOptionService)
        {
            _context = context;
            _user = user;
            _paymentOptionService = paymentOptionService;
        }

        public async Task<CustomerPayment> AddPaymentAsync(CustomerPaymentInputViewModel model)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            var userId = await _user.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");

            var defaultPaymentMethodId = await _paymentOptionService.GetDefaultIdAsync();

            var customerRemaining = await _context.Sales
                .Where(s => s.CustomerId == model.CustomerId && s.TenantId == tenantId)
                .SumAsync(s => (s.TotalAmount - s.ReturnedAmount - s.PaidAmount) < 0
                    ? 0
                    : s.TotalAmount - s.ReturnedAmount - s.PaidAmount);
            if (customerRemaining <= 0)
            {
                throw new InvalidOperationException("لا يوجد رصيد مستحق على هذا العميل.");
            }
            var discountAmount = model.DiscountAmount ?? 0m;
            if (model.Amount < 0 || discountAmount < 0)
            {
                throw new InvalidOperationException("قيمة المبلغ أو الخصم غير صحيحة.");
            }
            var totalIntent = model.Amount + discountAmount;
            if (totalIntent > customerRemaining)
            {
                throw new InvalidOperationException($"إجمالي (الدفعة + الخصم) أكبر من المتبقي على العميل: {customerRemaining:C}");
            }

            var payment = new CustomerPayment
            {
                CustomerId = model.CustomerId,
                Amount = model.Amount,
                PaymentDate = model.PaymentDate ?? DateTime.Now,
                PaymentMethodId = model.PaymentMethodId ?? defaultPaymentMethodId,
                Notes = model.Notes,
                TenantId = tenantId,
            };
            _context.CustomerPayments.Add(payment);
            await _context.SaveChangesAsync();

            // أولاً: طبق الخصم المباشر على أقدم الفواتير المفتوحة
            var discountRemaining = discountAmount;
            // ثم: وزع الدفعة النقدية
            var remaining = model.Amount;
            var openSales = await _context.Sales
                .Where(s => s.CustomerId == model.CustomerId 
                        && s.TenantId == tenantId 
                        && (s.TotalAmount - s.ReturnedAmount - s.PaidAmount) > 0)
                .OrderBy(s => s.SaleDate)
                .ThenBy(s => s.Id)
                .ToListAsync();

            foreach (var sale in openSales)
            {
                var saleRemaining = Math.Max(0, sale.TotalAmount - sale.ReturnedAmount - sale.PaidAmount);
                if (saleRemaining <= 0) continue;
                // خصم مباشر أولاً
                if (discountRemaining > 0)
                {
                    var discountToApply = Math.Min(discountRemaining, saleRemaining);
                    // لا نسجل PaymentAllocation للخصم لأنه ليس مبلغ مدفوع، لكن نعدّل PaidAmount
                    sale.PaidAmount += discountToApply;
                    saleRemaining -= discountToApply;
                    discountRemaining -= discountToApply;
                }

                var toAllocate = Math.Min(remaining, saleRemaining);
                if (toAllocate <= 0) break;

                var allocation = new PaymentAllocation
                {
                    CustomerPaymentId = payment.Id,
                    SaleId = sale.Id,
                    Amount = toAllocate
                };
                _context.PaymentAllocations.Add(allocation);

                // حدّث PaidAmount للفاتورة
                sale.PaidAmount += toAllocate;
                _context.Sales.Update(sale);

                remaining -= toAllocate;
                if (remaining <= 0) break;
            }

            // قيد إيراد في حساب المحل باسم "دفعة عميل" (النقد فقط)
            if (model.Amount > 0)
            {
                var customer = await _context.Customers.FindAsync(model.CustomerId);
                var storeTx = new StoreAccount
                {
                    TransactionName = $"دفعة عميل - {customer?.FullName}",
                    TransactionType = TransactionType.Income,
                    Amount = model.Amount,
                    TransactionDate = payment.PaymentDate,
                    Description = $"دفعة لحساب العميل {customer?.FullName}" + (discountAmount > 0 ? $" + خصم {discountAmount:C}" : string.Empty),
                    PaymentMethodId = payment.PaymentMethodId,
                    ReferenceNumber = $"CPAY-{payment.Id}",
                    TenantId = tenantId,
                };
                _context.StoreAccounts.Add(storeTx);
            }
            // لا نسجل قيد خصم منفصل في الخزنة؛ الخصم يُطبق فقط على رصيد العميل

            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            // احذف الدفعة وإلغاء أي تأثيرات مترتبة عليها
            var tenantId = await _user.GetCurrentTenantIdAsync();
            var payment = await _context.CustomerPayments
                .Include(p => p.Allocations)
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.Customer.TenantId == tenantId);
            if (payment == null)
                return false;

            // أنقص المبالغ المخصصة من PaidAmount للفواتير المرتبطة
            if (payment.Allocations != null && payment.Allocations.Any())
            {
                var saleIds = payment.Allocations.Select(a => a.SaleId).Distinct().ToList();
                var sales = await _context.Sales.Where(s => saleIds.Contains(s.Id)).ToListAsync();
                foreach (var alloc in payment.Allocations)
                {
                    var sale = sales.FirstOrDefault(s => s.Id == alloc.SaleId);
                    if (sale != null)
                    {
                        sale.PaidAmount = Math.Max(0, sale.PaidAmount - alloc.Amount);
                        _context.Sales.Update(sale);
                    }
                }
                _context.PaymentAllocations.RemoveRange(payment.Allocations);
            }

            // احذف قيد الخزنة المرتبط بهذه الدفعة إن وجد
            var storeTx = await _context.StoreAccounts
                .FirstOrDefaultAsync(sa => sa.ReferenceNumber == $"CPAY-{payment.Id}");
            if (storeTx != null)
            {
                _context.StoreAccounts.Remove(storeTx);
            }

            _context.CustomerPayments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CustomerStatementViewModel> GetCustomerStatementAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            
            var salesQuery = _context.Sales
                .Where(s => s.CustomerId == customerId && s.TenantId == tenantId)
                .AsQueryable();
                                
            var paymentsQuery = _context.CustomerPayments
                .Where(p => p.CustomerId == customerId && p.Customer.TenantId == tenantId)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                salesQuery = salesQuery.Where(s => s.SaleDate >= fromDate.Value);
                paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                salesQuery = salesQuery.Where(s => s.SaleDate <= toDate.Value);
                paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= toDate.Value);
            }

            var totalItems = await salesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var sales = await salesQuery
                .OrderBy(s => s.SaleDate)
                .ThenBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var payments = await paymentsQuery.OrderBy(p => p.PaymentDate).ToListAsync();

            // حساب إجمالي الفواتير المبيعات (بدون خصم المرتجعات)
            var totalSalesAmount = await salesQuery.SumAsync(s => s.TotalAmount);
            
            // حساب إجمالي الفواتير المرتجعات
            
            // حساب صافي المبيعات (المبيعات - المرتجعات)
            var netSalesAmount = totalSalesAmount;
            
            // احتساب الإجماليات الصافية لكل فاتورة: (بعد الخصم - المرتجع)
            var totalSalesNet = sales.Sum(s => Math.Max(0, s.TotalAmount - s.ReturnedAmount));
            var totalPaid = sales.Sum(s => s.PaidAmount);
            var balance = GetCustomerBalanceAsync(customerId).Result;

            return new CustomerStatementViewModel
            {
                CustomerId = customerId,
                FromDate = fromDate,
                ToDate = toDate,
                Sales = sales,
                Payments = payments,
                TotalSales = totalSalesNet,
                TotalPaid = totalPaid,
                Balance = balance,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                TotalSalesAmount = totalSalesAmount, // إجمالي الفواتير المبيعات
                NetSalesAmount = netSalesAmount // صافي المبيعات
            };
        }

        public async Task<decimal> GetCustomerBalanceAsync(int customerId)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            var totalSalesNet = await _context.Sales
                .Where(s => s.CustomerId == customerId && s.TenantId == tenantId)
                .SumAsync(s => (s.TotalAmount - s.ReturnedAmount) < 0
                    ? 0
                    : (s.TotalAmount - s.ReturnedAmount));
            var totalPaid = await _context.Sales
                .Where(s => s.CustomerId == customerId && s.TenantId == tenantId)
                .SumAsync(s => s.PaidAmount);
            return Math.Max(0, totalSalesNet - totalPaid);
        }

        public async Task<CustomerFullAccountViewModel> GetCustomerFullAccountAsync(int customerId, int page = 1, int pageSize = 20)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId && c.TenantId == tenantId);
                
            var saleEntries = await _context.Sales
                .Where(s => s.CustomerId == customerId && s.TenantId == tenantId)
                .Select(s => new CustomerAccountEntry
                {
                    Date = s.SaleDate,
                    Type = "فاتورة",
                    Amount = s.TotalAmount,
                    SaleId = s.Id,
                    Notes = $"مدفوع: {s.PaidAmount:C}, مرتجع: {s.ReturnedAmount:C}, متبقي: {s.RemainingAmount:C}",
                    SalePaymentTypeName = s.PaymentType.ToString()
                }).ToListAsync();

            var paymentEntries = await _context.CustomerPayments
                .Where(p => p.CustomerId == customerId && p.Customer.TenantId == tenantId)
                .Select(p => new CustomerAccountEntry
                {
                    Date = p.PaymentDate,
                    Type = "دفعة",
                    Amount = p.Amount,
                    PaymentId = p.Id,
                    Notes = p.Notes,
                    PaymentMethodName = p.PaymentMethod != null ? p.PaymentMethod.Name : null
                }).ToListAsync();

            // Attach allocation details for payment entries
            var paymentIds = paymentEntries.Select(p => p.PaymentId!.Value).ToList();
            if (paymentIds.Any())
            {
                var allocations = await _context.PaymentAllocations
                    .Where(a => paymentIds.Contains(a.CustomerPaymentId))
                    .GroupBy(a => a.CustomerPaymentId)
                    .Select(g => new { PaymentId = g.Key, Items = g.Select(x => new PaymentAllocationSummary { SaleId = x.SaleId, Amount = x.Amount }).ToList() })
                    .ToListAsync();
                foreach (var p in paymentEntries)
                {
                    var alloc = allocations.FirstOrDefault(a => a.PaymentId == p.PaymentId)?.Items;
                    if (alloc != null) p.Allocations = alloc;
                }
            }

            // Merge and sort chronologically. If two entries share the exact same timestamp,
      

            var all = saleEntries
                .Concat(paymentEntries)
                .OrderBy(e => e.Date)
                // لضمان وضوح الترتيب عند تساوي التاريخ: الفواتير أولاً ثم المرتجعات ثم الدفعات
                .ThenBy(e => e.Type == "فاتورة" ? 0 : (e.Type == "مرتجع" ? 1 : 2))
                .ThenBy(e => e.SaleId ?? e.ReturnId ?? e.PaymentId ?? int.MaxValue)
                .ToList();

            // Compute running totals (treat returns as negative sales)
            decimal runningSales = 0m, runningPaid = 0m;
            foreach (var e in all)
            {
                if (e.Type == "فاتورة") runningSales += e.Amount;
                else if (e.Type == "دفعة") runningPaid += e.Amount;
                else if (e.Type == "مرتجع") runningSales -= e.Amount;
                e.RunningSales = runningSales;
                e.RunningPaid = runningPaid;
                e.RunningBalance = Math.Max(0, runningSales - runningPaid);
            }

            var totalItems = all.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // حساب إجمالي الفواتير المبيعات (بدون خصم المرتجعات)
            var totalSalesAmount = await _context.Sales
                .Where(s => s.CustomerId == customerId && s.TenantId == tenantId)
                .SumAsync(s => s.TotalAmount);

            // حساب صافي المبيعات (المبيعات - المرتجعات)
            var netSalesAmount = totalSalesAmount;

            // حساب إجمالي المدفوع من صافي المبيعات (المبيعات - المرتجعات)
            var totalPaid2 = await _context.Sales
                .Where(s => s.CustomerId == customerId && s.TenantId == tenantId)
                .SumAsync(s => s.PaidAmount);
            
            // حساب المتبقي (صافي المبيعات - المدفوع)
            var balance = Math.Max(0, netSalesAmount - totalPaid2);

            return new CustomerFullAccountViewModel
            {
                CustomerId = customerId,
                CustomerName = customer?.FullName ?? ($"عميل #{customerId}"),
                CustomerPhone = customer?.PhoneNumber,
                Entries = paged,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                TotalSales = netSalesAmount, // صافي المبيعات
                TotalPaid = totalPaid2,
                Balance = balance,
                TotalSalesAmount = totalSalesAmount, // إجمالي الفواتير المبيعات
                NetSalesAmount = netSalesAmount // صافي المبيعات
            };
        }
    }
}


