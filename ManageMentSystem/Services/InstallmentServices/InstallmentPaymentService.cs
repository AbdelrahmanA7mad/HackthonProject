using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.PaymentOptionServices;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace ManageMentSystem.Services.InstallmentServices
{
    public class InstallmentPaymentService : IInstallmentPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IPaymentOptionService _paymentOptionService;
        private readonly IUserService _user;

        public InstallmentPaymentService(AppDbContext context, IPaymentOptionService paymentOptionService, IUserService user)
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

        public async Task<IEnumerable<InstallmentPayment>> GetAllPaymentsAsync()
        {
            var tenantId = await GetCurrentTenantId();

            return await _context.InstallmentPayments
                .Include(p => p.Installment)
                    .ThenInclude(i => i.Customer)
                .Include(p => p.Installment)
                    .ThenInclude(i => i.InstallmentItems)
                        .ThenInclude(ii => ii.Product)
                .Where(p => p.Installment.TenantId == tenantId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InstallmentPayment>> GetPaymentsByInstallmentIdAsync(int installmentId)
        {
            var tenantId = await GetCurrentTenantId();

            return await _context.InstallmentPayments
                .Include(p => p.Installment)
                .Where(p => p.InstallmentId == installmentId && p.Installment.TenantId == tenantId)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<InstallmentPayment> GetPaymentByIdAsync(int id)
        {
            var tenantId = await GetCurrentTenantId();

            return await _context.InstallmentPayments
                .Include(p => p.Installment)
                    .ThenInclude(i => i.Customer)
                .Include(p => p.Installment)
                    .ThenInclude(i => i.InstallmentItems)
                        .ThenInclude(ii => ii.Product)
                .Where(p => p.Id == id && p.Installment.TenantId == tenantId)
                .FirstOrDefaultAsync();
        }

        public async Task<InstallmentPayment> AddPaymentAsync(CreateInstallmentPaymentViewModel model)
        {
            var tenantId = await GetCurrentTenantId();
            var userId = await GetCurrentUserId();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");

            var installment = await _context.Installments
                .Where(i => i.Id == model.InstallmentId && i.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (installment == null)
                throw new InvalidOperationException("التقسيط غير موجود أو لا تملك صلاحية الوصول إليه");

            // Check if payment amount exceeds remaining amount
            var totalPaid = await GetTotalPaidForInstallmentAsync(model.InstallmentId);
            var totalPaidIncludingDownPayment = totalPaid + installment.DownPayment;
            var totalRequired = installment.TotalWithInterest + installment.ExtraMonthAmount;
            var remainingAmount = totalRequired - totalPaidIncludingDownPayment;

            var defaultPaymentMethodId = await _paymentOptionService.GetDefaultIdAsync();
            var selectedPaymentMethodId = model.PaymentMethodId ?? defaultPaymentMethodId;

            if (model.Amount > remainingAmount)
                throw new InvalidOperationException($"المبلغ المدخل ({model.Amount:C}) أكبر من المبلغ المتبقي ({remainingAmount:C})");

            var payment = new InstallmentPayment
            {
                InstallmentId = model.InstallmentId,
                Amount = model.Amount,
                PaymentDate = model.PaymentDate,
                PaymentMethodId = selectedPaymentMethodId,
                Notes = model.Notes ?? "",
                TenantId = tenantId,
            };

            _context.InstallmentPayments.Add(payment);
            await _context.SaveChangesAsync();

            // إنشاء عملية إيراد في حساب المحل لدفع القسط
            var installmentWithDetails = await _context.Installments
                .Where(i => i.Id == model.InstallmentId && i.TenantId == tenantId)
                .Include(i => i.Customer)
                .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync();

      

            if (installmentWithDetails != null)
            {
                var storeTransaction = new StoreAccount
                {
                    TransactionName = $"قسط - {installmentWithDetails.Customer?.FullName ?? "عميل"}",
                    TransactionType = TransactionType.Income,
                    Amount = model.Amount,
                    TransactionDate = model.PaymentDate,
                    Description = $"دفع قسط للعميل {installmentWithDetails.Customer?.FullName} - المنتجات: {string.Join(", ", installmentWithDetails.InstallmentItems.Select(ii => ii.Product?.Name ?? "غير محدد"))} - المبلغ: {model.Amount:C}",
                    Category = "الأقساط",
                    PaymentMethodId = selectedPaymentMethodId,
                    ReferenceNumber = $"INST-PAYMENT-{payment.Id}",
                    InstallmentPaymentId = payment.Id,
                    TenantId = tenantId,
                };

                _context.StoreAccounts.Add(storeTransaction);
                await _context.SaveChangesAsync();
            }

            // Update installment total paid and remaining amount
            var newTotalPaid = totalPaidIncludingDownPayment + model.Amount;
            installment.TotalPaid = newTotalPaid;
            installment.RemainingAmount = Math.Max(0, totalRequired - newTotalPaid);

            // Update installment status using the same logic as GetInstallmentStatus
            if (installment.TotalPaid >= totalRequired)
            {
                installment.Status = "مكتمل";
                installment.CompletionDate = DateTime.Now;
            }
            else
            {
                // Get all payments for this installment to calculate status correctly
                var allPayments = await _context.InstallmentPayments
                    .Where(p => p.InstallmentId == model.InstallmentId)
                    .ToListAsync();

                // Calculate status using the same logic as GetInstallmentStatus
                var totalPaymentsAfterDownPayment = allPayments.Sum(p => p.Amount);
                var paidMonths = 0;
                if (installment.MonthlyPayment > 0)
                {
                    paidMonths = (int)(totalPaymentsAfterDownPayment / installment.MonthlyPayment);
                }

                var lastPaidMonthDate = installment.StartDate.AddMonths(paidMonths);
                var nextMonthRequired = (paidMonths + 1) * installment.MonthlyPayment;

                // Handle extra month
                if (installment.HasExtraMonth && paidMonths >= installment.NumberOfMonths)
                {
                    var extraMonthRequired = installment.NumberOfMonths * installment.MonthlyPayment + installment.ExtraMonthAmount;
                    nextMonthRequired = extraMonthRequired;
                }

                var isOverdue = lastPaidMonthDate < DateTime.Today && totalPaymentsAfterDownPayment < nextMonthRequired;

                installment.Status = isOverdue ? "متأخر" : "نشط";
            }

            _context.Installments.Update(installment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<InstallmentPayment> UpdatePaymentAsync(int id, CreateInstallmentPaymentViewModel model)
        {
            var tenantId = await GetCurrentTenantId();

            var payment = await _context.InstallmentPayments
                .Include(p => p.Installment)
                .Where(p => p.Id == id && p.Installment.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (payment == null)
                return null;

            // تحديث العملية المرتبطة في حساب المحل
            var storeTransaction = await _context.StoreAccounts
                .Where(sa => sa.InstallmentPaymentId == payment.Id && sa.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (storeTransaction != null)
            {
                var installmentWithDetails = await _context.Installments
                    .Where(i => i.Id == payment.InstallmentId && i.TenantId == tenantId)
                    .Include(i => i.Customer)
                    .Include(i => i.InstallmentItems)
                        .ThenInclude(ii => ii.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (installmentWithDetails != null)
                {
                    storeTransaction.TransactionName = $"قسط - {installmentWithDetails.Customer?.FullName ?? "عميل"}";
                    storeTransaction.Amount = model.Amount;
                    storeTransaction.TransactionDate = model.PaymentDate;
                    storeTransaction.Description = $"دفع قسط للعميل {installmentWithDetails.Customer?.FullName} - المنتجات: {string.Join(", ", installmentWithDetails.InstallmentItems.Select(ii => ii.Product?.Name ?? "غير محدد"))} - المبلغ: {model.Amount:C}";

                    _context.StoreAccounts.Update(storeTransaction);
                }
            }

            var defaultPaymentMethodId = await _paymentOptionService.GetDefaultIdAsync();
            var selectedPaymentMethodId = model.PaymentMethodId ?? defaultPaymentMethodId;

            // Recalculate installment totals
            var installment = await _context.Installments
                .Where(i => i.Id == payment.InstallmentId && i.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (installment == null)
                return null;

            var oldAmount = payment.Amount;
            var newAmount = model.Amount;

            payment.Amount = model.Amount;
            payment.PaymentDate = model.PaymentDate;
            payment.PaymentMethodId = selectedPaymentMethodId;
            payment.Notes = model.Notes ?? "";

            // Recalculate total paid including down payment
            var totalPaid = await GetTotalPaidForInstallmentAsync(payment.InstallmentId);
            var newTotalPaid = totalPaid + installment.DownPayment;
            installment.TotalPaid = newTotalPaid;
            installment.RemainingAmount = Math.Max(0, installment.TotalWithInterest - newTotalPaid);

            // Update installment status using the same logic as GetInstallmentStatus
            var totalRequired = installment.TotalWithInterest + installment.ExtraMonthAmount;
            if (installment.TotalPaid >= totalRequired)
            {
                installment.Status = "مكتمل";
                installment.CompletionDate = DateTime.Now;
            }
            else
            {
                // Get all payments for this installment to calculate status correctly
                var allPayments = await _context.InstallmentPayments
                    .Where(p => p.InstallmentId == payment.InstallmentId)
                    .ToListAsync();

                // Calculate status using the same logic as GetInstallmentStatus
                var totalPaymentsAfterDownPayment = allPayments.Sum(p => p.Amount);
                var paidMonths = 0;
                if (installment.MonthlyPayment > 0)
                {
                    paidMonths = (int)(totalPaymentsAfterDownPayment / installment.MonthlyPayment);
                }

                var lastPaidMonthDate = installment.StartDate.AddMonths(paidMonths);
                var nextMonthRequired = (paidMonths + 1) * installment.MonthlyPayment;

                // Handle extra month
                if (installment.HasExtraMonth && paidMonths >= installment.NumberOfMonths)
                {
                    var extraMonthRequired = installment.NumberOfMonths * installment.MonthlyPayment + installment.ExtraMonthAmount;
                    nextMonthRequired = extraMonthRequired;
                }

                var isOverdue = lastPaidMonthDate < DateTime.Today && totalPaymentsAfterDownPayment < nextMonthRequired;

                installment.Status = isOverdue ? "متأخر" : "نشط";
                installment.CompletionDate = null;
            }

            _context.InstallmentPayments.Update(payment);
            _context.Installments.Update(installment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            var tenantId = await GetCurrentTenantId();

            var payment = await _context.InstallmentPayments
                .Include(p => p.Installment)
                .Where(p => p.Id == id && p.Installment.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (payment == null)
                return false;

            // حذف العملية المرتبطة من حساب المحل
            var storeTransaction = await _context.StoreAccounts
                .Where(sa => sa.InstallmentPaymentId == payment.Id && sa.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (storeTransaction != null)
            {
                _context.StoreAccounts.Remove(storeTransaction);
            }

            var installment = await _context.Installments
                .Where(i => i.Id == payment.InstallmentId && i.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (installment == null)
                return false;

            // Recalculate total paid including down payment (after removing this payment)
            var totalPaid = await GetTotalPaidForInstallmentAsync(payment.InstallmentId);
            var newTotalPaid = totalPaid + installment.DownPayment;
            var totalRequired = installment.TotalWithInterest + installment.ExtraMonthAmount;
            installment.TotalPaid = newTotalPaid;
            installment.RemainingAmount = Math.Max(0, totalRequired - newTotalPaid);

            // Update installment status using the same logic as GetInstallmentStatus
            if (installment.TotalPaid >= totalRequired)
            {
                installment.Status = "مكتمل";
                installment.CompletionDate = DateTime.Now;
            }
            else
            {
                // Get all payments for this installment to calculate status correctly
                var allPayments = await _context.InstallmentPayments
                    .Where(p => p.InstallmentId == payment.InstallmentId)
                    .ToListAsync();

                // Calculate status using the same logic as GetInstallmentStatus
                var totalPaymentsAfterDownPayment = allPayments.Sum(p => p.Amount);
                var paidMonths = 0;
                if (installment.MonthlyPayment > 0)
                {
                    paidMonths = (int)(totalPaymentsAfterDownPayment / installment.MonthlyPayment);
                }

                var lastPaidMonthDate = installment.StartDate.AddMonths(paidMonths);
                var nextMonthRequired = (paidMonths + 1) * installment.MonthlyPayment;

                // Handle extra month
                if (installment.HasExtraMonth && paidMonths >= installment.NumberOfMonths)
                {
                    var extraMonthRequired = installment.NumberOfMonths * installment.MonthlyPayment + installment.ExtraMonthAmount;
                    nextMonthRequired = extraMonthRequired;
                }

                var isOverdue = lastPaidMonthDate < DateTime.Today && totalPaymentsAfterDownPayment < nextMonthRequired;

                installment.Status = isOverdue ? "متأخر" : "نشط";
                installment.CompletionDate = null;
            }

            _context.InstallmentPayments.Remove(payment);
            _context.Installments.Update(installment);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetTotalPaidForInstallmentAsync(int installmentId)
        {
            var tenantId = await GetCurrentTenantId();

            // التحقق من أن القسط ينتمي للمستخدم
            var installmentExists = await _context.Installments
                .Where(i => i.Id == installmentId && i.TenantId == tenantId)
                .AnyAsync();

            if (!installmentExists)
                return 0;

            return await _context.InstallmentPayments
                .Where(p => p.InstallmentId == installmentId)
                .SumAsync(p => p.Amount);
        }

        public async Task<int> GetPaidMonthsForInstallmentAsync(int installmentId)
        {
            var tenantId = await GetCurrentTenantId();

            var installment = await _context.Installments
                .Where(i => i.Id == installmentId && i.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (installment == null || installment.MonthlyPayment == 0)
                return 0;

            var totalPaid = await GetTotalPaidForInstallmentAsync(installmentId);

            // Check if this installment has been rescheduled
            if (installment.RescheduleDate.HasValue)
            {
                // This is a rescheduled installment
                // Calculate payments made after rescheduling
                var paymentsAfterReschedule = Math.Max(0, totalPaid - installment.TotalPaidBeforeReschedule);

                // Calculate paid months based on payments after rescheduling
                var rescheduledPaidMonths = (int)Math.Floor(paymentsAfterReschedule / installment.MonthlyPayment);
                return Math.Min(rescheduledPaidMonths, installment.NumberOfMonths);
            }

            // For non-rescheduled installments, calculate normally
            var totalPaymentsAfterDownPayment = totalPaid;
            var paidMonths = (int)Math.Floor(totalPaymentsAfterDownPayment / installment.MonthlyPayment);

            // Check if extra month is also paid
            if (installment.HasExtraMonth && paidMonths >= installment.NumberOfMonths)
            {
                var totalRequiredForExtraMonth = installment.NumberOfMonths * installment.MonthlyPayment + installment.ExtraMonthAmount;
                if (totalPaymentsAfterDownPayment >= totalRequiredForExtraMonth)
                {
                    paidMonths = installment.NumberOfMonths + 1; // Include extra month
                }
                else
                {
                    paidMonths = installment.NumberOfMonths; // Regular months only
                }
            }
            else
            {
                // Make sure we don't exceed the total number of months
                paidMonths = Math.Min(paidMonths, installment.NumberOfMonths);
            }

            return paidMonths;
        }

        public async Task<CreateInstallmentPaymentViewModel> GetInstallmentDetailsForPaymentAsync(int installmentId)
        {
            var tenantId = await GetCurrentTenantId();

            var installment = await _context.Installments
                .Where(i => i.Id == installmentId && i.TenantId == tenantId)
                .Include(i => i.Customer)
                .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                .FirstOrDefaultAsync();

            if (installment == null)
                return null;

            var totalPaid = await GetTotalPaidForInstallmentAsync(installmentId);
            var totalPaidIncludingDownPayment = totalPaid + installment.DownPayment;
            var totalRequired = installment.TotalWithInterest + installment.ExtraMonthAmount;
            var paidMonths = await GetPaidMonthsForInstallmentAsync(installmentId);
            var totalMonthsCount = installment.NumberOfMonths + (installment.HasExtraMonth ? 1 : 0);
            var remainingMonths = Math.Max(0, totalMonthsCount - paidMonths);
            var nextMonthToPay = Math.Min(paidMonths + 1, totalMonthsCount);
            var nextMonthDueDate = installment.StartDate.AddMonths(nextMonthToPay);

            return new CreateInstallmentPaymentViewModel
            {
                InstallmentId = installmentId,
                CustomerName = installment.Customer?.FullName,
                ProductName = installment.InstallmentItems?.Any() == true
                    ? string.Join(", ", installment.InstallmentItems.Select(ii => ii.Product?.Name ?? "غير محدد"))
                    : "لا توجد منتجات",
                MonthlyPayment = installment.MonthlyPayment,
                TotalPaid = totalPaidIncludingDownPayment,
                RemainingAmount = Math.Max(0, totalRequired - totalPaidIncludingDownPayment),
                NumberOfMonths = totalMonthsCount,
                PaidMonths = paidMonths,
                RemainingMonths = remainingMonths,
                NextMonthToPay = nextMonthToPay,
                NextMonthDueDate = nextMonthDueDate
            };
        }

    }
}
