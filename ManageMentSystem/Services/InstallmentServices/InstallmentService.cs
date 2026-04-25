using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.PaymentOptionServices;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Services.InstallmentServices
{
    public class InstallmentService : IInstallmentService
    {
        private readonly AppDbContext _context;
        private readonly IPaymentOptionService _paymentOptionService;
        private readonly IUserService _user;

        public InstallmentService(AppDbContext context, IPaymentOptionService paymentOptionService, IUserService user)
        {
            _context = context;
            _paymentOptionService = paymentOptionService;
            _user = user;
        }

        // دالة للحصول على TenantId الحالي
        private async Task<string> GetCurrentTenantId()
        {
            return await _user.GetCurrentTenantIdAsync();
        }

        // دالة للحصول على UserId الحالي
        private async Task<string> GetCurrentUserId()
        {
            return await _user.GetCurrentUserIdAsync();
        }

        // دالة لحساب حالة القسط بناءً على الدفعات
        private string GetInstallmentStatus(Installment installment, List<InstallmentPayment> payments)
        {
            var totalMonthsCount = installment.NumberOfMonths + (installment.HasExtraMonth ? 1 : 0);
            var isAnyMonthOverdue = false;
            var totalPaymentsAfterDownPayment = payments.Sum(p => p.Amount);

            for (int month = 1; month <= totalMonthsCount; month++)
            {
                var dueDate = installment.StartDate.AddMonths(month);
                var amountNeededUpToThisMonth = month * installment.MonthlyPayment;
                var isExtraMonth = month > installment.NumberOfMonths;
                bool isPaid;

                if (isExtraMonth)
                {
                    var totalRequiredForExtraMonth = installment.NumberOfMonths * installment.MonthlyPayment + installment.ExtraMonthAmount;
                    isPaid = totalPaymentsAfterDownPayment >= totalRequiredForExtraMonth;
                }
                else
                {
                    isPaid = totalPaymentsAfterDownPayment >= amountNeededUpToThisMonth;
                }

                var isOverdue = dueDate < DateTime.Today && !isPaid;
                if (isOverdue)
                {
                    isAnyMonthOverdue = true;
                    break;
                }
            }

            var totalPaid = payments.Sum(p => p.Amount);
            var totalPaidWithDown = totalPaid + installment.DownPayment;
            var totalRequired = installment.TotalWithInterest + installment.ExtraMonthAmount;

            if (totalPaidWithDown >= totalRequired)
            {
                return "مكتمل";
            }
            else if (isAnyMonthOverdue)
            {
                return "متأخر";
            }
            else
            {
                return "نشط";
            }
        }

        public async Task<List<Installment>> GetAllInstallmentsAsync()
        {
            var tenantId = await GetCurrentTenantId();

            var installments = await _context.Installments
                .Where(i => i.TenantId == tenantId)
                .Include(i => i.Customer)
                .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                .Include(i => i.Payments)
                .OrderByDescending(i => i.StartDate)
                .ToListAsync();

            foreach (var installment in installments)
            {
                if (installment.Status == "نشط" || installment.Status == "متأخر")
                {
                    var payments = installment.Payments.ToList();
                    var newStatus = GetInstallmentStatus(installment, payments);
                    if (installment.Status != newStatus)
                    {
                        installment.Status = newStatus;
                        installment.CompletionDate = (newStatus == "مكتمل") ? DateTime.Now : null;
                        _context.Installments.Update(installment);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return installments;
        }

        public async Task<Installment> GetInstallmentByIdAsync(int id)
        {
            var tenantId = await GetCurrentTenantId();

            return await _context.Installments
                .Where(i => i.Id == id && i.TenantId == tenantId)
                .Include(i => i.Customer)
                .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync();
        }

        public async Task<Installment> AddInstallmentAsync(CreateInstallmentViewModel model)
        {
            try
            {
                var tenantId = await GetCurrentTenantId();
                var userId = await GetCurrentUserId();
                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                    throw new InvalidOperationException("المستخدم غير مسجل دخول");

                // التحقق من وجود عناصر
                if (model.Items == null || !model.Items.Any())
                {
                    throw new InvalidOperationException("يجب إضافة منتج واحد على الأقل");
                }

                // التحقق من توفر المخزون لجميع المنتجات - مع فلتر UserId
                foreach (var item in model.Items)
                {
                    if (!item.ProductId.HasValue)
                    {
                        throw new InvalidOperationException("معرف المنتج مطلوب لجميع العناصر");
                    }

                    var product = await _context.Products
                        .Where(p => p.Id == item.ProductId.Value && p.TenantId == tenantId)
                        .FirstOrDefaultAsync();

                    if (product == null)
                    {
                        throw new InvalidOperationException($"المنتج غير موجود");
                    }

                    if (product.Quantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"المنتج '{product.Name}' غير متوفر في المخزون بالكمية المطلوبة");
                    }
                }

                // التحقق من أن العميل ينتمي للمستخدم الحالي
                var customer = await _context.Customers
                    .Where(c => c.Id == model.CustomerId && c.TenantId == tenantId)
                    .FirstOrDefaultAsync();

                if (customer == null)
                {
                    throw new InvalidOperationException("العميل غير موجود أو لا تملك صلاحية الوصول إليه");
                }

                // Calculate interest and totals
                var remainingAmount = model.TotalAmount - model.DownPayment;
                var interestAmount = Math.Round(remainingAmount * (model.InterestRate / 100) * (model.NumberOfMonths / 12m), 0);
                var totalWithInterest = model.TotalAmount + interestAmount;
                var monthlyPaymentWithInterest = Math.Round((remainingAmount + interestAmount) / model.NumberOfMonths, 0);

                // Calculate extra month amount if enabled
                var extraMonthAmount = 0m;
                if (model.HasExtraMonth)
                {
                    extraMonthAmount = monthlyPaymentWithInterest;
                }

                // Determine next sequence number - مع فلتر TenantId
                var lastSequenceNumber = await _context.Installments
                    .Where(i => i.TenantId == tenantId)
                    .MaxAsync(i => (int?)i.SequenceNumber) ?? 0;
                var nextSequenceNumber = lastSequenceNumber + 1;

                var installment = new Installment
                {
                    SequenceNumber = nextSequenceNumber,
                    CustomerId = model.CustomerId,
                    TenantId = tenantId,
                    TotalAmount = model.TotalAmount,
                    DownPayment = model.DownPayment,
                    MonthlyPayment = monthlyPaymentWithInterest,
                    NumberOfMonths = model.NumberOfMonths,
                    StartDate = model.StartDate,
                    Status = "نشط",
                    InterestRate = model.InterestRate,
                    InterestAmount = interestAmount,
                    TotalWithInterest = totalWithInterest,
                    RemainingAmount = remainingAmount + interestAmount + extraMonthAmount,
                    TotalPaid = model.DownPayment,
                    HasExtraMonth = model.HasExtraMonth,
                    ExtraMonthAmount = extraMonthAmount,
                    GuarantorName = model.GuarantorName,
                    GuarantorPhone = model.GuarantorPhone
                };

                _context.Installments.Add(installment);
                await _context.SaveChangesAsync();

                // إنشاء العناصر وتحديث المخزون
                foreach (var item in model.Items)
                {
                    var product = await _context.Products
                        .Where(p => p.Id == item.ProductId.Value && p.TenantId == tenantId)
                        .FirstOrDefaultAsync();

                    var installmentItem = new InstallmentItem
                    {
                        InstallmentId = installment.Id,
                        ProductId = item.ProductId.Value,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice,
                        Description = item.Description,
                        TenantId = tenantId
                    };

                    _context.InstallmentItems.Add(installmentItem);

                    // تحديث المخزون
                    product.Quantity -= item.Quantity;
                    _context.Products.Update(product);
                }

                await _context.SaveChangesAsync();

                // إنشاء عملية إيراد في حساب المحل للدفعة المقدمة
                var defaultPaymentMethodId = await _paymentOptionService.GetDefaultIdAsync();
                var selectedPaymentMethodId = model.PaymentMethodId ?? defaultPaymentMethodId;

                if (model.DownPayment > 0)
                {
                    var productNames = string.Join(", ", model.Items.Select(i => i.ProductName ?? "منتج"));
                    var storeTransaction = new StoreAccount
                    {
                        TransactionName = $"تقسيط - {customer.FullName}",
                        TransactionType = TransactionType.Income,
                        Amount = model.DownPayment,
                        TransactionDate = model.StartDate,
                        Description = $"دفعة مقدمة للتقسيط - العميل: {customer.FullName} - المنتجات: {productNames} - المبلغ: {model.DownPayment:C}",
                        Category = "الأقساط",
                        PaymentMethodId = selectedPaymentMethodId,
                        ReferenceNumber = $"INST-DOWN-{installment.Id}",
                        TenantId = tenantId,
                    };

                    _context.StoreAccounts.Add(storeTransaction);
                    await _context.SaveChangesAsync();
                }

                return installment;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<bool> RescheduleInstallmentAsync(int id, int newMonths)
        {
            var tenantId = await GetCurrentTenantId();

            var installment = await _context.Installments
                .Where(i => i.Id == id && i.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (installment == null)
                return false;

            // Reuse logic from Update but with existing values
            var totalPaidSoFar = await _context.InstallmentPayments
                .Where(p => p.InstallmentId == installment.Id)
                .SumAsync(p => p.Amount);

            var remainingAmount = installment.TotalAmount - installment.DownPayment;
            var originalNumberOfMonths = installment.NumberOfMonths;
            
            // Recalculate interest based on original principal and rate, but NEW duration
            var interestAmount = Math.Round(remainingAmount * (installment.InterestRate / 100) * (newMonths / 12m), 0);
            
            // Calculate new monthly payment
            // Logic derived from UpdateInstallmentAsync:
            // "if isReschedule { remainingAmountAfterPayments = remainingAmount - totalPaidSoFar; monthly = (remainingAmountAfterPayments + interestAmount) / newMonths }"
            // Note: UpdateInstallmentAsync logic seems to treat interest as being fully re-added/re-calculated on the *remaining principal*.
            // Wait, looking at UpdateInstallmentAsync: it calculates `interestAmount` based on `remainingAmount` (Principal - DownPayment).
            // Then if rescheduling, it takes (Principal - DownPayment - PaidSoFar) + interestAmount. 
            // This implies the interest is calculated on the FULL principal again for the new duration? 
            // The original code was: var interestAmount = Math.Round(remainingAmount * (model.InterestRate / 100) * (model.NumberOfMonths / 12m), 0);
            // Yes, it recalculates total interest based on total principal for the new duration.
            
            var remainingAmountAfterPayments = remainingAmount - totalPaidSoFar;
            var monthlyPaymentWithInterest = Math.Round((remainingAmountAfterPayments + interestAmount) / newMonths, 0);

            var extraMonthAmount = 0m;
            if (installment.HasExtraMonth)
            {
                extraMonthAmount = monthlyPaymentWithInterest;
                installment.ExtraMonthAmount = extraMonthAmount;
            }

            installment.NumberOfMonths = newMonths;
            installment.InterestAmount = interestAmount;
            installment.TotalWithInterest = installment.TotalAmount + interestAmount;
            installment.MonthlyPayment = monthlyPaymentWithInterest;
            
            // Update remaining amount
            // Logic from UpdateInstallmentAsync for isReschedule:
            installment.RemainingAmount = remainingAmountAfterPayments + interestAmount + extraMonthAmount;
            installment.RescheduleDate = DateTime.Now;
            installment.TotalPaidBeforeReschedule = totalPaidSoFar;

            _context.Installments.Update(installment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteInstallmentAsync(int id)
        {
            try
            {
                var tenantId = await GetCurrentTenantId();

                var installment = await _context.Installments
                    .Where(i => i.Id == id && i.TenantId == tenantId)
                    .Include(i => i.InstallmentItems)
                        .ThenInclude(ii => ii.Product)
                    .FirstOrDefaultAsync();

                if (installment == null)
                    return false;

                // التعامل مع StoreAccount للدفعة المقدمة
                var downPaymentTransaction = await _context.StoreAccounts
                    .Where(sa => sa.ReferenceNumber == $"INST-DOWN-{installment.Id}" && sa.TenantId == tenantId)
                    .FirstOrDefaultAsync();

                if (downPaymentTransaction != null)
                {
                    // دائماً حذف مع إلغاء العملية بالكامل
                    _context.StoreAccounts.Remove(downPaymentTransaction);
                }

                // حذف جميع دفعات القسط المرتبطة
                var payments = await _context.InstallmentPayments
                    .Where(p => p.InstallmentId == id)
                    .ToListAsync();
                _context.InstallmentPayments.RemoveRange(payments);

                // التعامل مع عمليات دفع الأقساط من حساب المحل
                var paymentTransactions = await _context.StoreAccounts
                    .Where(sa => sa.InstallmentPaymentId.HasValue &&
                                payments.Select(p => p.Id).Contains(sa.InstallmentPaymentId.Value) &&
                                sa.TenantId == tenantId)
                    .ToListAsync();

                // دائماً حذف مع إلغاء العملية بالكامل
                _context.StoreAccounts.RemoveRange(paymentTransactions);

                // زيادة كمية المنتجات - مع فلتر UserId
                var installmentItems = await _context.InstallmentItems
                    .Where(ii => ii.InstallmentId == id)
                    .Include(ii => ii.Product)
                    .ToListAsync();

                foreach (var item in installmentItems)
                {
                    if (item.Product != null && item.Product.TenantId == tenantId)
                    {
                        item.Product.Quantity += item.Quantity;
                        _context.Products.Update(item.Product);
                    }
                }

                // حذف عناصر القسط
                var itemsToDelete = await _context.InstallmentItems
                    .Where(ii => ii.InstallmentId == id)
                    .ToListAsync();
                _context.InstallmentItems.RemoveRange(itemsToDelete);

                // حذف القسط
                _context.Installments.Remove(installment);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<Installment>> GetInstallmentsWithDetailsAsync()
        {
            var tenantId = await GetCurrentTenantId();

            return await _context.Installments
                .Where(i => i.TenantId == tenantId)
                .Include(i => i.Customer)
                .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                .OrderByDescending(i => i.StartDate)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var tenantId = await GetCurrentTenantId();

            return await _context.Customers
                .Where(c => c.TenantId == tenantId)
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var tenantId = await GetCurrentTenantId();

            return await _context.Products
                .Where(p => p.Quantity > 0 && p.TenantId == tenantId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            try
            {
                var tenantId = await GetCurrentTenantId();
                var userId = await GetCurrentUserId();
                customer.TenantId = tenantId;

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return customer;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task UpdateInstallmentRemainingAmountAsync(int installmentId)
        {
            var tenantId = await GetCurrentTenantId();

            var installment = await _context.Installments
                .Where(i => i.Id == installmentId && i.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (installment == null)
                return;

            var totalPaid = await _context.InstallmentPayments
                .Where(p => p.InstallmentId == installmentId)
                .SumAsync(p => p.Amount);

            installment.TotalPaid = totalPaid + installment.DownPayment;
            var totalRequired = installment.TotalWithInterest + installment.ExtraMonthAmount;
            installment.RemainingAmount = Math.Max(0, totalRequired - installment.TotalPaid);

            _context.Installments.Update(installment);
            await _context.SaveChangesAsync();
        }

        public async Task<InstallmentDetailsViewModel> GetInstallmentDetailsWithMonthlyPaymentsAsync(int id)
        {
            var tenantId = await GetCurrentTenantId();

            var installment = await _context.Installments
                .Where(i => i.Id == id && i.TenantId == tenantId)
                .Include(i => i.Customer)
                .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync();

            if (installment == null)
                return null;

            var viewModel = new InstallmentDetailsViewModel
            {
                Installment = installment,
                MonthlyPayments = new List<MonthlyPaymentStatus>()
            };

            if (installment.RescheduleDate.HasValue)
            {
                viewModel.IsRescheduled = true;
                viewModel.TotalPaidBeforeReschedule = installment.TotalPaidBeforeReschedule;
                viewModel.PaidMonthsBeforeReschedule = (int)Math.Floor(installment.TotalPaidBeforeReschedule / installment.MonthlyPayment);
                viewModel.OriginalMonthlyPayment = installment.MonthlyPayment;
                viewModel.OriginalNumberOfMonths = installment.NumberOfMonths;
            }

            var totalMonthsCount = installment.NumberOfMonths + (installment.HasExtraMonth ? 1 : 0);

            for (int month = 1; month <= totalMonthsCount; month++)
            {
                var dueDate = installment.StartDate.AddMonths(month);
                var monthStart = installment.StartDate.AddMonths(month - 1);
                var monthEnd = installment.StartDate.AddMonths(month);

                var monthPayments = installment.Payments
                    .Where(p => p.PaymentDate >= monthStart && p.PaymentDate < monthEnd)
                    .ToList();

                var totalPaidForMonth = monthPayments.Sum(p => p.Amount);
                var totalPaymentsAfterDownPayment = installment.Payments.Sum(p => p.Amount);
                var amountNeededUpToThisMonth = month * installment.MonthlyPayment;
                var isExtraMonth = month > installment.NumberOfMonths;
                bool isPaid;

                if (viewModel.IsRescheduled)
                {
                    var paymentsAfterReschedule = Math.Max(0, totalPaymentsAfterDownPayment - installment.TotalPaidBeforeReschedule);

                    if (isExtraMonth)
                    {
                        var totalRequiredForExtraMonth = installment.NumberOfMonths * installment.MonthlyPayment + installment.ExtraMonthAmount;
                        isPaid = paymentsAfterReschedule >= totalRequiredForExtraMonth;
                    }
                    else
                    {
                        isPaid = paymentsAfterReschedule >= amountNeededUpToThisMonth;
                    }
                }
                else
                {
                    if (isExtraMonth)
                    {
                        var totalRequiredForExtraMonth = installment.NumberOfMonths * installment.MonthlyPayment + installment.ExtraMonthAmount;
                        isPaid = totalPaymentsAfterDownPayment >= totalRequiredForExtraMonth;
                    }
                    else
                    {
                        isPaid = totalPaymentsAfterDownPayment >= amountNeededUpToThisMonth;
                    }
                }

                var isOverdue = dueDate < DateTime.Today && !isPaid;

                string status;
                if (isPaid)
                    status = "مدفوع";
                else if (isOverdue)
                    status = "متأخر";
                else
                    status = "غير مدفوع";

                var monthAmount = isExtraMonth ? installment.ExtraMonthAmount : installment.MonthlyPayment;

                viewModel.MonthlyPayments.Add(new MonthlyPaymentStatus
                {
                    MonthNumber = month,
                    DueDate = dueDate,
                    Amount = monthAmount,
                    IsPaid = isPaid,
                    PaymentDate = monthPayments.Any() ? monthPayments.Max(p => p.PaymentDate) : null,
                    PaidAmount = totalPaidForMonth > 0 ? totalPaidForMonth : null,
                    Status = status,
                    IsExtraMonth = isExtraMonth
                });
            }

            return viewModel;
        }

        public async Task<bool> AddExtraMonthAsync(int installmentId)
        {
            var tenantId = await GetCurrentTenantId();

            var installment = await _context.Installments
                .Where(i => i.Id == installmentId && i.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (installment == null)
                return false;

            if (installment.HasExtraMonth)
                return false;

            installment.HasExtraMonth = true;
            installment.ExtraMonthAmount = installment.MonthlyPayment;
            installment.RemainingAmount += installment.ExtraMonthAmount;

            _context.Installments.Update(installment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveExtraMonthAsync(int installmentId)
        {
            var tenantId = await GetCurrentTenantId();

            var installment = await _context.Installments
                .Where(i => i.Id == installmentId && i.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (installment == null)
                return false;

            if (!installment.HasExtraMonth)
                return false;

            installment.HasExtraMonth = false;
            installment.RemainingAmount = Math.Max(0, installment.RemainingAmount - installment.ExtraMonthAmount);
            installment.ExtraMonthAmount = 0;

            _context.Installments.Update(installment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateExtraMonthAmountAsync(int installmentId)
        {
            var tenantId = await GetCurrentTenantId();

            var installment = await _context.Installments
                .Where(i => i.Id == installmentId && i.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (installment == null)
                return 0;

            return installment.MonthlyPayment;
        }

        public async Task<InstallmentSummaryViewModel> GetInstallmentSummaryAsync()
        {
            var tenantId = await GetCurrentTenantId();
            await UpdateOverdueInstallmentsAsync();

            var installments = await _context.Installments
                .Where(i => i.TenantId == tenantId)
                .Include(i => i.Customer)
                .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                .Include(i => i.Payments)
                .AsNoTracking()
                .ToListAsync();

            return await CalculateInstallmentSummary(installments);
        }

        public async Task<List<Installment>> GetFilteredInstallmentsAsync(DateTime startDate, DateTime endDate, string filterType)
        {
            var tenantId = await GetCurrentTenantId();

            IQueryable<Installment> query = _context.Installments
                .Where(i => i.TenantId == tenantId)
                .Include(i => i.Customer)
                .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                .Include(i => i.Payments);

            switch (filterType.ToLower())
            {
                case "created":
                    query = query.Where(i => i.StartDate >= startDate && i.StartDate <= endDate);
                    break;
                case "due":
                    query = query.Where(i =>
                        Enumerable.Range(1, i.NumberOfMonths + (i.HasExtraMonth ? 1 : 0))
                        .Any(month =>
                            i.StartDate.AddMonths(month) >= startDate &&
                            i.StartDate.AddMonths(month) <= endDate
                        )
                    );
                    break;
                case "payment":
                    query = query.Where(i =>
                        i.Payments.Any(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                    );
                    break;
                default:
                    query = query.Where(i => i.StartDate >= startDate && i.StartDate <= endDate);
                    break;
            }

            var installments = await query
                .OrderByDescending(i => i.StartDate)
                .ToListAsync();

            foreach (var installment in installments)
            {
                if (installment.Status == "نشط" || installment.Status == "متأخر")
                {
                    var payments = installment.Payments.ToList();
                    var newStatus = GetInstallmentStatus(installment, payments);
                    if (installment.Status != newStatus)
                    {
                        installment.Status = newStatus;
                        installment.CompletionDate = (newStatus == "مكتمل") ? DateTime.Now : null;
                        _context.Installments.Update(installment);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return installments;
        }

        public async Task<InstallmentSummaryViewModel> GetFilteredInstallmentSummaryAsync(DateTime startDate, DateTime endDate, string filterType)
        {
            var installments = await GetFilteredInstallmentsAsync(startDate, endDate, filterType);
            return await CalculateInstallmentSummary(installments);
        }

        private async Task<InstallmentSummaryViewModel> CalculateInstallmentSummary(List<Installment> installments)
        {
            var summary = new InstallmentSummaryViewModel();

            if (!installments.Any())
                return summary;

            // إحصائيات أساسية
            summary.TotalInstallments = installments.Count;
            summary.ActiveInstallments = installments.Count(i => i.Status == "نشط");
            summary.CompletedInstallments = installments.Count(i => i.Status == "مكتمل");
            summary.OverdueInstallments = installments.Count(i => i.Status == "متأخر");

            // إحصائيات المبالغ الأساسية
            summary.TotalBaseAmount = installments.Sum(i => i.TotalAmount);
            summary.TotalAmountWithInterest = installments.Sum(i => i.TotalWithInterest);
            summary.TotalAmountWithoutExtraMonth = installments.Sum(i => i.TotalWithInterest);
            summary.TotalAmountWithExtraMonth = installments.Sum(i => i.TotalWithInterest + i.ExtraMonthAmount);

            // إحصائيات الفوائد
            summary.TotalInterest = installments.Sum(i => i.InterestAmount);
            summary.AverageInterestRate = installments.Any() ? installments.Average(i => i.InterestRate) : 0;

            // إحصائيات الدفعات
            summary.TotalDownPayments = installments.Sum(i => i.DownPayment);
            summary.TotalMonthlyPayments = installments.Sum(i => i.MonthlyPayment * i.NumberOfMonths);
            summary.TotalExtraMonths = installments.Where(i => i.HasExtraMonth).Sum(i => i.ExtraMonthAmount);
            summary.TotalExtraMonthsApplied = installments.Count(i => i.HasExtraMonth);

            // حساب المبالغ المدفوعة
            var totalPaid = installments.Sum(i => i.Payments?.Sum(p => p.Amount) ?? 0);
            summary.TotalAmountPaid = totalPaid + summary.TotalDownPayments;
            summary.TotalAmountRemaining = summary.TotalAmountWithExtraMonth - summary.TotalAmountPaid;

            // حساب الدفعات الشهرية المدفوعة والمتبقية
            summary.TotalMonthlyPaymentsPaid = installments.Sum(i =>
            {
                var actualPayments = i.Payments?.Sum(p => p.Amount) ?? 0;
                var totalRequiredForMonthlyPayments = i.TotalWithInterest - i.DownPayment;

                if (actualPayments >= totalRequiredForMonthlyPayments)
                {
                    return totalRequiredForMonthlyPayments;
                }

                return actualPayments;
            });

            summary.TotalMonthlyPaymentsRemaining = installments.Sum(i =>
            {
                var totalRequiredForMonthlyPayments = i.TotalWithInterest - i.DownPayment;
                var actualPayments = i.Payments?.Sum(p => p.Amount) ?? 0;
                var remainingAmount = totalRequiredForMonthlyPayments - actualPayments;

                return Math.Max(0, remainingAmount);
            });

            // إحصائيات إضافية
            summary.AverageMonthlyPayment = installments.Any() ? installments.Average(i => i.MonthlyPayment) : 0;
            summary.LargestInstallment = installments.Any() ? installments.Max(i => i.TotalWithInterest + i.ExtraMonthAmount) : 0;
            summary.SmallestInstallment = installments.Any() ? installments.Min(i => i.TotalWithInterest + i.ExtraMonthAmount) : 0;

            // إحصائيات العملاء والمنتجات
            summary.TotalCustomers = installments.Select(i => i.CustomerId).Distinct().Count();
            summary.TotalProducts = installments.SelectMany(i => i.InstallmentItems).Select(ii => ii.ProductId).Distinct().Count();

            // حساب النسب المئوية
            summary.CompletionRate = summary.TotalInstallments > 0 ? (double)summary.CompletedInstallments / summary.TotalInstallments * 100 : 0;
            summary.OverdueRate = summary.TotalInstallments > 0 ? (double)summary.OverdueInstallments / summary.TotalInstallments * 100 : 0;
            summary.PaymentPercentage = summary.TotalAmountWithExtraMonth > 0 ? (double)(summary.TotalAmountPaid / summary.TotalAmountWithExtraMonth * 100) : 0;
            summary.RemainingPercentage = summary.TotalAmountWithExtraMonth > 0 ? (double)(summary.TotalAmountRemaining / summary.TotalAmountWithExtraMonth * 100) : 0;

            // حساب الأشهر
            summary.TotalPaidMonths = installments.Sum(i =>
            {
                var totalPaidForInstallment = (i.Payments?.Sum(p => p.Amount) ?? 0) + i.DownPayment;
                var monthlyPayment = i.MonthlyPayment;
                var totalMonths = i.NumberOfMonths + (i.HasExtraMonth ? 1 : 0);

                if (monthlyPayment <= 0) return 0;

                var paidMonths = (int)(totalPaidForInstallment / monthlyPayment);
                return Math.Min(paidMonths, totalMonths);
            });

            summary.TotalRemainingMonths = installments.Sum(i =>
            {
                var totalPaidForInstallment = (i.Payments?.Sum(p => p.Amount) ?? 0) + i.DownPayment;
                var totalRequired = i.TotalWithInterest + i.ExtraMonthAmount;
                var remainingAmount = totalRequired - totalPaidForInstallment;
                var monthlyPayment = i.MonthlyPayment;

                if (monthlyPayment <= 0) return 0;

                return (int)Math.Ceiling(remainingAmount / monthlyPayment);
            });

            return summary;
        }

        public async Task<PaginatedInstallmentsViewModel> GetPaginatedInstallmentsAsync(int pageNumber, int pageSize, string searchTerm = "", string sortBy = "StartDate", string sortOrder = "desc")
        {
            var tenantId = await GetCurrentTenantId();

            if (pageNumber == 1)
            {
                await UpdateOverdueInstallmentsAsync();
            }

            var query = _context.Installments
                .Where(i => i.TenantId == tenantId)
                .Include(i => i.Customer)
                .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                .Include(i => i.Payments)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                if (int.TryParse(searchTerm, out var numberSearch))
                {
                    query = query.Where(i =>
                        i.SequenceNumber == numberSearch ||
                        i.Id == numberSearch ||
                        i.Customer.FullName.ToLower().Contains(searchTermLower) ||
                        i.InstallmentItems.Any(ii => ii.Product.Name.ToLower().Contains(searchTermLower)) ||
                        i.Status.ToLower().Contains(searchTermLower)
                    );
                }
                else
                {
                    query = query.Where(i =>
                        i.Customer.FullName.ToLower().Contains(searchTermLower) ||
                        i.InstallmentItems.Any(ii => ii.Product.Name.ToLower().Contains(searchTermLower)) ||
                        i.Status.ToLower().Contains(searchTermLower)
                    );
                }
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "sequencenumber" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(i => i.SequenceNumber)
                    : query.OrderByDescending(i => i.SequenceNumber),
                "customername" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(i => i.Customer.FullName)
                    : query.OrderByDescending(i => i.Customer.FullName),
                "totalamount" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(i => i.TotalAmount)
                    : query.OrderByDescending(i => i.TotalAmount),
                "status" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(i => i.Status)
                    : query.OrderByDescending(i => i.Status),
                "startdate" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(i => i.StartDate)
                    : query.OrderByDescending(i => i.StartDate),
                _ => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(i => i.StartDate)
                    : query.OrderByDescending(i => i.StartDate)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var installments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedInstallmentsViewModel
            {
                Installments = installments,
                CurrentPage = pageNumber,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }

        public async Task UpdateOverdueInstallmentsAsync()
        {
            var tenantId = await GetCurrentTenantId();

            var activeInstallments = await _context.Installments
                .Where(i => i.TenantId == tenantId && (i.Status == "نشط" || i.Status == "متأخر"))
                .Include(i => i.Payments)
                .ToListAsync();

            var updatedCount = 0;
            foreach (var installment in activeInstallments)
            {
                var payments = installment.Payments.ToList();
                var newStatus = GetInstallmentStatus(installment, payments);

                if (installment.Status != newStatus)
                {
                    installment.Status = newStatus;
                    installment.CompletionDate = (newStatus == "مكتمل") ? DateTime.Now : null;
                    _context.Installments.Update(installment);
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Installment>> GetOverdueInstallmentsAsync()
        {
            var tenantId = await GetCurrentTenantId();

            return await _context.Installments
                .Where(i => i.TenantId == tenantId && i.Status == "متأخر")
                .Include(i => i.Customer)
                .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                .OrderBy(i => i.StartDate)
                .ToListAsync();
        }
    }
}
