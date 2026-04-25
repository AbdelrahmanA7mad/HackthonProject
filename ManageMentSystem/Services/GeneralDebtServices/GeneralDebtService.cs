using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Services.StoreAccountServices;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.Services.PaymentOptionServices;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ManageMentSystem.Services.GeneralDebtServices
{
    public class GeneralDebtService : IGeneralDebtService
    {
        private readonly AppDbContext _context;
        private readonly IStoreAccountService _storeAccountService;
        private readonly IUserService _user;
        private readonly IPaymentOptionService _paymentOptionService;

        public GeneralDebtService(
            AppDbContext context,
            IStoreAccountService storeAccountService,
            IUserService user,
            IPaymentOptionService paymentOptionService)
        {
            _context = context;
            _storeAccountService = storeAccountService;
            _user = user;
            _paymentOptionService = paymentOptionService;
        }

        public async Task<List<GeneralDebt>> GetAllAsync()
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            return await _context.GeneralDebts
                .Include(gd => gd.StoreAccounts)
                .Where(gd => gd.TenantId == tenantId)
                .OrderByDescending(gd => gd.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PaymentMethodOption>> GetPaymentMethodsAsync()
        {
            return await _paymentOptionService.GetActiveAsync();
        }

        public async Task<GeneralDebt?> GetByIdAsync(int id)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            return await _context.GeneralDebts
                .Include(gd => gd.StoreAccounts)
                .FirstOrDefaultAsync(gd => gd.Id == id && gd.TenantId == tenantId);
        }

        public async Task<(GeneralDebt debt, string? infoMessage)> CreateAsync(CreateGeneralDebtViewModel model)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            var userId = await _user.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");

            var debt = new GeneralDebt
            {
                Title = model.Title,
                PartyName = model.PartyName,
                DebtType = model.DebtType,
                Amount = model.Amount,
                PaidAmount = 0,
                DueDate = model.DueDate,
                Description = model.Description,
                CreatedAt = DateTime.Now,
                TenantId = tenantId,
            };

            // لو الدين ليا (أنا أديت فلوس لحد وهو مدين ليا بيها)
            if (model.DebtType == GeneralDebtType.OwedToMe)
            {
                decimal cashPart;
                decimal debtPart;
                if (model.PaymentMethodId.HasValue)
                {
                    var methodBalance = await _storeAccountService.GetCashBalanceByPaymentMethodAsync(model.PaymentMethodId);
                    if (methodBalance <= 0)
                        throw new InvalidOperationException("لا يوجد رصيد متاح في وسيلة الدفع المختارة.");
                    if (model.Amount > methodBalance)
                        throw new InvalidOperationException($"الرصيد غير كافٍ في وسيلة الدفع المختارة. الرصيد الحالي: {methodBalance:C}، والمبلغ المطلوب: {model.Amount:C}");

                    cashPart = model.Amount;
                    debtPart = 0;
                }
                else
                {
                    var currentCash = await _storeAccountService.GetCashBalanceAsync();
                    cashPart = Math.Min(currentCash, model.Amount);
                    debtPart = model.Amount - cashPart;
                }

                // خصم الكاش اللي أديته للشخص (لو فيه كاش متاح)
                StoreAccount? transaction = null;
                if (cashPart > 0)
                {
                    var defaultPaymentMethodOptionId = await _paymentOptionService.GetDefaultIdAsync();

                    var saModelCash = new StoreAccountViewModel
                    {
                        TransactionName = $"إقراض نقدي - {model.Title}",
                        TransactionType = TransactionType.Expense,
                        Amount = cashPart,
                        TransactionDate = DateTime.Now,
                        Description = model.Description,
                        Category = "ديون عامة",
                        PaymentMethodId = model.PaymentMethodId ?? defaultPaymentMethodOptionId,
                        ReferenceNumber = $"GENDEBT-LEND-{DateTime.Now.Ticks}"
                    };
                    transaction = await _storeAccountService.CreateTransactionAsync(saModelCash);
                }

                debt.Amount = model.Amount;
                debt.PaidAmount = 0;

                // ربط الدين بالـ transaction
                if (transaction != null)
                {
                    debt.StoreAccounts = new List<StoreAccount> { transaction };
                }

                // لو فيه جزء مقدرتش أديه نقدي، أنشئ دين إضافي عليا
                if (debtPart > 0)
                {
                    var storeDebt = new GeneralDebt
                    {
                        Title = $"دين على المحل مقابل إقراض - {model.Title}",
                        PartyName = "المحل",
                        DebtType = GeneralDebtType.OnMe,
                        Amount = debtPart,
                        PaidAmount = 0,
                        CreatedAt = DateTime.Now,
                        DueDate = model.DueDate,
                        Description = $"المبلغ المتبقي من إقراض: {model.Description}",
                        TenantId = tenantId,
                    };
                    _context.GeneralDebts.Add(storeDebt);
                }
            }
            // لو الدين عليا (أنا مدين لحد - استلمت فلوس منه)
            else
            {
                var defaultPaymentMethodOptionId2 = await _paymentOptionService.GetDefaultIdAsync();

                var saModel = new StoreAccountViewModel
                {
                    TransactionName = $"استلام مبلغ مقابل دين - {model.Title}",
                    TransactionType = TransactionType.Income,
                    Amount = model.Amount,
                    TransactionDate = DateTime.Now,
                    Description = model.Description,
                    Category = "ديون عامة",
                    PaymentMethodId = model.PaymentMethodId ?? defaultPaymentMethodOptionId2,
                    ReferenceNumber = $"GENDEBT-BORROW-{DateTime.Now.Ticks}"
                };
                var transaction = await _storeAccountService.CreateTransactionAsync(saModel);

                debt.Amount = model.Amount;
                debt.PaidAmount = 0;

                // ربط الدين بالـ transaction
                debt.StoreAccounts = new List<StoreAccount> { transaction };
            }

            _context.GeneralDebts.Add(debt);
            await _context.SaveChangesAsync();

            // تحضير رسالة معلوماتية
            string? infoMessage = null;
            if (model.DebtType == GeneralDebtType.OwedToMe)
            {
                var currentCash = await _storeAccountService.GetCashBalanceAsync();
                var cashPart = Math.Min(currentCash, model.Amount);
                var debtPart = model.Amount - cashPart;

                if (cashPart > 0 && debtPart > 0)
                {
                    infoMessage = $"تم إقراض {cashPart:C} نقداً، والمبلغ المتبقي {debtPart:C} تم تسجيله كدين على المحل";
                }
                else if (cashPart == 0)
                {
                    infoMessage = $"لا يوجد كاش متاح. تم تسجيل المبلغ كاملاً ({model.Amount:C}) كدين على المحل";
                }
            }
            else
            {
                infoMessage = $"تم استلام {model.Amount:C} وإضافته إلى كاش المحل مع تسجيل الدين";
            }

            return (debt, infoMessage);
        }

        public async Task<GeneralDebt> UpdateAsync(int id, CreateGeneralDebtViewModel model)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            var debt = await _context.GeneralDebts
                .Include(d => d.StoreAccounts)
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId)
                ?? throw new ArgumentException("الدين غير موجود");

            // لو فيه transactions مرتبطة بالدين وفيه تغيير في المبلغ
            if (debt.StoreAccounts?.Any() == true && debt.Amount != model.Amount)
            {
                var initialTransaction = debt.StoreAccounts.FirstOrDefault();
                if (initialTransaction != null)
                {
                    // حساب الفرق
                    var difference = model.Amount - debt.Amount;

                    // تحديث المبلغ في الـ transaction
                    initialTransaction.Amount = model.Amount;
                    _context.StoreAccounts.Update(initialTransaction);
                }
            }

            debt.Title = model.Title;
            debt.PartyName = model.PartyName;
            debt.DebtType = model.DebtType;
            debt.Amount = model.Amount;
            debt.DueDate = model.DueDate;
            debt.Description = model.Description;

            await _context.SaveChangesAsync();
            return debt;
        }

        public async Task DeleteAsync(int id)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            var debt = await _context.GeneralDebts
                .Include(d => d.StoreAccounts)
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId);

            if (debt != null)
            {
                // حذف الـ transactions المرتبطة أولاً
                if (debt.StoreAccounts?.Any() == true)
                {
                    foreach (var transaction in debt.StoreAccounts.ToList())
                    {
                        _context.StoreAccounts.Remove(transaction);
                    }
                }

                _context.GeneralDebts.Remove(debt);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(decimal residual, string? warningMessage)> AddPaymentAsync(int id, decimal amount, int? paymentMethodId, string? description = null)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();
            var userId = await _user.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");

            var debt = await _context.GeneralDebts
                .Include(d => d.StoreAccounts)
                .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == tenantId)
                ?? throw new ArgumentException("الدين غير موجود");

            if (amount <= 0) throw new ArgumentException("المبلغ غير صالح");

            var remaining = debt.Amount - debt.PaidAmount;
            if (remaining <= 0)
                return (0m, "هذا الدين تم سداده بالكامل بالفعل");

            if (amount > remaining)
                return (0m, $"المبلغ المدخل ({amount:C}) أكبر من المبلغ المتبقي ({remaining:C}). يرجى إدخال مبلغ أقل أو مساوي للمتبقي");

            // لو الدين عليا (أنا مدين لحد وعاوز أسدد)
            if (debt.DebtType == GeneralDebtType.OnMe)
            {
                decimal cashPart;
                decimal debtPart;
                if (paymentMethodId.HasValue)
                {
                    var methodBalance = await _storeAccountService.GetCashBalanceByPaymentMethodAsync(paymentMethodId);
                    if (methodBalance <= 0)
                        throw new InvalidOperationException("لا يوجد رصيد متاح في وسيلة الدفع المختارة.");
                    if (amount > methodBalance)
                        throw new InvalidOperationException($"الرصيد غير كافٍ في وسيلة الدفع المختارة. الرصيد الحالي: {methodBalance:C}، والمبلغ المطلوب: {amount:C}");
                    cashPart = amount;
                    debtPart = 0;
                }
                else
                {
                    var currentCash = await _storeAccountService.GetCashBalanceAsync();
                    cashPart = Math.Min(currentCash, amount);
                    debtPart = amount - cashPart;
                }

                // دفع بالكاش المتاح
                if (cashPart > 0)
                {
                    var defaultPaymentMethodOptionId3 = await _paymentOptionService.GetDefaultIdAsync();

                    var saModelCash = new StoreAccountViewModel
                    {
                        TransactionName = $"سداد دين نقدي - {debt.Title}",
                        TransactionType = TransactionType.Expense,
                        Amount = cashPart,
                        TransactionDate = DateTime.Now,
                        Description = description ?? debt.Description,
                        Category = "ديون عامة",
                        PaymentMethodId = paymentMethodId ?? defaultPaymentMethodOptionId3,
                        ReferenceNumber = $"GENDEBT-PAY-{debt.Id}-{DateTime.Now.Ticks}"
                    };
                    var transaction = await _storeAccountService.CreateTransactionAsync(saModelCash);

                    // ربط الدفعة بالدين
                    debt.StoreAccounts ??= new List<StoreAccount>();
                    debt.StoreAccounts.Add(transaction);

                    debt.PaidAmount += cashPart;
                }

                // لو فيه باقي مقدرش أدفعه نقدي
                if (debtPart > 0)
                {
                    var residualDebt = new GeneralDebt
                    {
                        Title = $"باقي سداد - {debt.Title}",
                        PartyName = debt.PartyName,
                        DebtType = GeneralDebtType.OnMe,
                        Amount = debtPart,
                        PaidAmount = 0,
                        CreatedAt = DateTime.Now,
                        DueDate = debt.DueDate,
                        Description = description ?? debt.Description,
                        TenantId = tenantId,
                    };
                    _context.GeneralDebts.Add(residualDebt);
                }

                await _context.SaveChangesAsync();
                return (debtPart, debtPart > 0 ? $"تم دفع {cashPart:C} نقداً، والمبلغ المتبقي {debtPart:C} تم تحويله كدين جديد على المحل" : null);
            }
            // لو الدين ليا (حد مدين ليا وبيسدد)
            else
            {
                debt.PaidAmount += amount;

                var defaultPaymentMethodOptionId4 = await _paymentOptionService.GetDefaultIdAsync();

                var saModel = new StoreAccountViewModel
                {
                    TransactionName = $"تحصيل دين - {debt.Title}",
                    TransactionType = TransactionType.Income,
                    Amount = amount,
                    TransactionDate = DateTime.Now,
                    Description = description ?? debt.Description,
                    Category = "ديون عامة",
                    PaymentMethodId = paymentMethodId ?? defaultPaymentMethodOptionId4,
                    ReferenceNumber = $"GENDEBT-COLLECT-{debt.Id}-{DateTime.Now.Ticks}"
                };
                var transaction = await _storeAccountService.CreateTransactionAsync(saModel);

                // ربط التحصيل بالدين
                debt.StoreAccounts ??= new List<StoreAccount>();
                debt.StoreAccounts.Add(transaction);

                await _context.SaveChangesAsync();
                return (0m, null);
            }
        }

        /// <summary>
        /// معالجة تعديل transaction من StoreAccount
        /// </summary>
        public async Task HandleStoreAccountUpdateAsync(int transactionId, decimal oldAmount, decimal newAmount)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();

            // البحث عن الدين المرتبط بهذا الـ transaction
            var debt = await _context.GeneralDebts
                .Include(d => d.StoreAccounts)
                .Where(d => d.TenantId == tenantId)
                .FirstOrDefaultAsync(d => d.StoreAccounts.Any(sa => sa.Id == transactionId));

            if (debt != null)
            {
                var difference = newAmount - oldAmount;

                // تحديث المبلغ المدفوع أو المبلغ الأساسي حسب نوع الدين
                if (debt.PaidAmount > 0)
                {
                    // لو كانت دفعة
                    debt.PaidAmount += difference;

                    // التأكد من عدم تجاوز المبلغ الأساسي
                    if (debt.PaidAmount > debt.Amount)
                        debt.PaidAmount = debt.Amount;
                    if (debt.PaidAmount < 0)
                        debt.PaidAmount = 0;
                }
                else
                {
                    // لو كان المبلغ الأساسي
                    debt.Amount = newAmount;
                }

                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// معالجة حذف transaction من StoreAccount
        /// </summary>
        public async Task HandleStoreAccountDeleteAsync(int transactionId)
        {
            var tenantId = await _user.GetCurrentTenantIdAsync();

            // البحث عن الدين المرتبط
            var debt = await _context.GeneralDebts
                .Include(d => d.StoreAccounts)
                .Where(d => d.TenantId == tenantId)
                .FirstOrDefaultAsync(d => d.StoreAccounts.Any(sa => sa.Id == transactionId));

            if (debt != null)
            {
                var transaction = debt.StoreAccounts.FirstOrDefault(sa => sa.Id == transactionId);
                if (transaction != null)
                {
                    // إرجاع المبلغ المدفوع
                    if (debt.PaidAmount > 0)
                    {
                        debt.PaidAmount -= transaction.Amount;
                        if (debt.PaidAmount < 0)
                            debt.PaidAmount = 0;
                    }

                    // إزالة العلاقة
                    debt.StoreAccounts.Remove(transaction);

                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}