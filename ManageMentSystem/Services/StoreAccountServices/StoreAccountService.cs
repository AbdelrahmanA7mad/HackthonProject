using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.Services.PaymentOptionServices;

namespace ManageMentSystem.Services.StoreAccountServices
{
    public class StoreAccountService : IStoreAccountService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly IPaymentOptionService _paymentOptionService;

        public StoreAccountService(AppDbContext context, IUserService userService, IPaymentOptionService paymentOptionService)
        {
            _context = context;
            _userService = userService;
            _paymentOptionService = paymentOptionService;
        }

        public async Task<List<StoreAccount>> GetAllTransactionsAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return new List<StoreAccount>();

            return await _context.StoreAccounts
                .Where(sa => sa.TenantId == tenantId)
                .Include(sa => sa.Sale)
                .Include(sa => sa.GeneralDebt)
                .Include(sa => sa.PaymentMethod)
                .OrderByDescending(sa => sa.TransactionDate)
                .AsNoTracking() // تحسين الأداء - عدم تتبع التغييرات
                .ToListAsync();
        }

        public async Task<StoreAccount?> GetTransactionByIdAsync(int id)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return null;

            return await _context.StoreAccounts
                .Where(sa => sa.TenantId == tenantId)
                .Include(sa => sa.Sale)
                .Include(sa => sa.GeneralDebt)
                .Include(sa => sa.PaymentMethod)
                .FirstOrDefaultAsync(sa => sa.Id == id);
        }

        public async Task<StoreAccount> CreateTransactionAsync(StoreAccountViewModel model)
        {
            if (model.TransactionType == TransactionType.Expense)
            {
                var balanceScope = await (model.PaymentMethodId.HasValue
                    ? GetCashBalanceByPaymentMethodAsync(model.PaymentMethodId)
                    : GetCashBalanceAsync());
                if (model.Amount > balanceScope)
                {
                    throw new InvalidOperationException($"لا يمكن تنفيذ العملية: المبلغ أكبر من الرصيد المتاح. الرصيد المتاح: {balanceScope:C}");
                }
            }


            // إنشاء عملية إيراد في حساب المحل
            var defaultPaymentMethodId = await _paymentOptionService.GetDefaultIdAsync() ?? 1;

            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");

            var transaction = new StoreAccount
            {
                TransactionName = model.TransactionName,
                TransactionType = model.TransactionType,
                Amount = model.Amount,
                TransactionDate = model.TransactionDate,
                Description = model.Description,
                Category = model.Category,
                PaymentMethodId = model.PaymentMethodId ?? defaultPaymentMethodId,
                ReferenceNumber = model.ReferenceNumber,
                Notes = model.Notes,
                Capital = 0,
                SaleId = model.SaleId,
                TenantId = tenantId,
            };

            _context.StoreAccounts.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<StoreAccount> UpdateTransactionAsync(int id, StoreAccountViewModel model)
        {
            var transaction = await _context.StoreAccounts.FindAsync(id);
            if (transaction == null)
                throw new ArgumentException("العملية غير موجودة");

            var currentBalance = await (model.PaymentMethodId.HasValue
                ? GetCashBalanceByPaymentMethodAsync(model.PaymentMethodId)
                : GetCashBalanceAsync());
            var balanceWithoutCurrentTransaction = currentBalance;

            if (transaction.TransactionType == TransactionType.Expense)
            {
                balanceWithoutCurrentTransaction += transaction.Amount;
            }
            else if (transaction.TransactionType == TransactionType.Income)
            {
                balanceWithoutCurrentTransaction -= transaction.Amount;
            }

            if (model.TransactionType == TransactionType.Expense)
            {
                if (model.Amount > balanceWithoutCurrentTransaction)
                {
                    throw new InvalidOperationException($"لا يمكن تنفيذ العملية: المبلغ أكبر من رصيد المحل المتاح. الرصيد المتاح: {balanceWithoutCurrentTransaction:C}");
                }
            }



            transaction.TransactionName = model.TransactionName;
            transaction.TransactionType = model.TransactionType;
            transaction.Amount = model.Amount;
            transaction.TransactionDate = model.TransactionDate;
            transaction.Description = model.Description;
            transaction.Category = model.Category;
            transaction.PaymentMethodId = model.PaymentMethodId;
            transaction.ReferenceNumber = model.ReferenceNumber;
            transaction.Notes = model.Notes;
            transaction.Capital = 0;
            transaction.SaleId = model.SaleId;
            
            // Ensure TenantId and CreatedByUserId are set
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");
            
            // Verify transaction belongs to current tenant
            if (transaction.TenantId != tenantId)
                throw new UnauthorizedAccessException("ليس لديك صلاحية لتعديل هذه العملية");
            
            if (string.IsNullOrEmpty(transaction.TenantId)) transaction.TenantId = tenantId;

            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task DeleteTransactionAsync(int id)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) throw new InvalidOperationException("المستخدم غير مسجل دخول");
            var transaction = await _context.StoreAccounts
                .FirstOrDefaultAsync(sa => sa.Id == id && sa.TenantId == tenantId);
            
            if (transaction != null)
            {
                _context.StoreAccounts.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("العملية غير موجودة أو ليس لديك صلاحية لحذفها");
            }
        }

        public async Task<StoreAccountSummaryViewModel> GetAccountSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return new StoreAccountSummaryViewModel();
            var query = _context.StoreAccounts.AsQueryable().Where(sa => sa.TenantId == tenantId);

            if (fromDate.HasValue)
                query = query.Where(sa => sa.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(sa => sa.TransactionDate <= toDate.Value);

            var transactions = await query
                .Include(sa => sa.Sale)
                .OrderByDescending(sa => sa.TransactionDate)
                .Take(20)
                .ToListAsync();

            var totalIncome = await GetTotalIncomeAsync(fromDate, toDate);
            var totalExpenses = await GetTotalExpensesAsync(fromDate, toDate);
            var totalCapital = await GetTotalCapitalAsync();

            var summary = new StoreAccountSummaryViewModel
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                TotalCapital = totalCapital,
                RecentTransactions = transactions.Select(t => new StoreAccountViewModel
                {
                    Id = t.Id,
                    TransactionName = t.TransactionName,
                    TransactionType = t.TransactionType,
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate,
                    Description = t.Description,
                    Category = t.Category,
                    PaymentMethodId = t.PaymentMethodId,
                    ReferenceNumber = t.ReferenceNumber,
                    Notes = t.Notes,
                    SaleId = t.SaleId
                }).ToList()
            };

            // Get monthly summaries for current year
            summary.MonthlySummaries = await GetMonthlySummaryAsync(DateTime.Now.Year);
            summary.CategorySummaries = await GetCategorySummaryAsync(fromDate, toDate);

            return summary;
        }

        public async Task<List<StoreAccount>> GetFilteredTransactionsAsync(StoreAccountFilterViewModel filter)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return new List<StoreAccount>();
            var query = _context.StoreAccounts.Where(sa => sa.TenantId == tenantId);

            if (filter.FromDate.HasValue)
                query = query.Where(sa => sa.TransactionDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(sa => sa.TransactionDate <= filter.ToDate.Value);

            if (filter.TransactionType.HasValue)
                query = query.Where(sa => sa.TransactionType == filter.TransactionType.Value);

            if (!string.IsNullOrEmpty(filter.Category))
                query = query.Where(sa => sa.Category == filter.Category);

            if (filter.PaymentMethodId.HasValue)
                query = query.Where(sa => sa.PaymentMethodId == filter.PaymentMethodId.Value);

            if (filter.MinAmount.HasValue)
                query = query.Where(sa => sa.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(sa => sa.Amount <= filter.MaxAmount.Value);

            return await query
                .Include(sa => sa.Sale)
                .Include(sa => sa.GeneralDebt)
                .Include(sa => sa.PaymentMethod)
                .OrderByDescending(sa => sa.TransactionDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalIncomeAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return 0m;
            var query = _context.StoreAccounts
                .Where(sa => sa.TransactionType == TransactionType.Income && sa.TenantId == tenantId);

            if (fromDate.HasValue)
                query = query.Where(sa => sa.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(sa => sa.TransactionDate <= toDate.Value);

            return await query.SumAsync(sa => sa.Amount);
        }

        public async Task<decimal> GetTotalExpensesAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return 0m;
            var query = _context.StoreAccounts
                .Where(sa => sa.TransactionType == TransactionType.Expense && sa.TenantId == tenantId);

            if (fromDate.HasValue)
                query = query.Where(sa => sa.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(sa => sa.TransactionDate <= toDate.Value);

            return await query.SumAsync(sa => sa.Amount);
        }

        public async Task<decimal> GetTotalCapitalAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return 0m;

            // 1. حساب قيمة المنتجات في المخزون (فقط منتجات المستخدم الحالي)
            var productsValue = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .SumAsync(p => p.PurchasePrice * p.Quantity);

            // 2. حساب الرصيد النقدي (الدخل - المصروفات)
            var totalIncome = await GetTotalIncomeAsync();
            var totalExpenses = await GetTotalExpensesAsync();
            var cashBalance = totalIncome - totalExpenses;

            // 3. رأس المال = قيمة المنتجات + الرصيد النقدي
            return productsValue + cashBalance;
        }

        public async Task<decimal> GetCashBalanceAsync()
        {
            var totalIncome = await GetTotalIncomeAsync();
            var totalExpenses = await GetTotalExpensesAsync();
            return totalIncome - totalExpenses;
        }

        public async Task<decimal> GetCashBalanceByPaymentMethodAsync(int? paymentMethodId)
        {
            if (!paymentMethodId.HasValue)
                return await GetCashBalanceAsync();

            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return 0m;
            
            var income = await _context.StoreAccounts
                .Where(sa => sa.TransactionType == TransactionType.Income 
                    && sa.PaymentMethodId == paymentMethodId
                    && sa.TenantId == tenantId)
                .SumAsync(sa => sa.Amount);
            
            var expenses = await _context.StoreAccounts
                .Where(sa => sa.TransactionType == TransactionType.Expense 
                    && sa.PaymentMethodId == paymentMethodId
                    && sa.TenantId == tenantId)
                .SumAsync(sa => sa.Amount);
            return income - expenses;
        }

        public async Task<List<MonthlySummary>> GetMonthlySummaryAsync(int year)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return new List<MonthlySummary>();
            var monthlyData = await _context.StoreAccounts
                .Where(sa => sa.TransactionDate.Year == year && sa.TenantId == tenantId)
                .GroupBy(sa => new { sa.TransactionDate.Year, sa.TransactionDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Income = g.Where(sa => sa.TransactionType == TransactionType.Income).Sum(sa => sa.Amount),
                    Expenses = g.Where(sa => sa.TransactionType == TransactionType.Expense).Sum(sa => sa.Amount)
                })
                .ToListAsync();

            var monthNames = new[] { "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو", 
                                   "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };

            return monthlyData.Select(m => new MonthlySummary
            {
                Month = monthNames[m.Month - 1],
                Income = m.Income,
                Expenses = m.Expenses
            }).ToList();
        }

        public async Task<List<CategorySummary>> GetCategorySummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return new List<CategorySummary>();
            var query = _context.StoreAccounts.Where(sa => sa.TenantId == tenantId);

            if (fromDate.HasValue)
                query = query.Where(sa => sa.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(sa => sa.TransactionDate <= toDate.Value);

            return await query
                .GroupBy(sa => new { sa.Category, sa.TransactionType })
                .Select(g => new CategorySummary
                {
                    Category = g.Key.Category ?? "غير محدد",
                    TotalAmount = g.Sum(sa => sa.Amount),
                    TransactionCount = g.Count(),
                    Type = g.Key.TransactionType
                })
                .ToListAsync();
        }

        public async Task AutoCreateTransactionFromSaleAsync(Sale sale)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");

            var defaultPaymentMethodId = await _context.PaymentMethodOptions
                .Where(pm => pm.IsDefault && pm.TenantId == tenantId)
                .Select(pm => (int?)pm.Id)
                .FirstOrDefaultAsync() ?? 1;

            var transaction = new StoreAccount
            {
                TransactionName = $"بيع - {sale.Customer?.FullName ?? "عميل"}",
                TransactionType = TransactionType.Income,
                Amount = sale.PaidAmount,
                TransactionDate = sale.SaleDate,
                Description = $"بيع منتجات للعميل {sale.Customer?.FullName}",
                Category = "المبيعات",
                PaymentMethodId = defaultPaymentMethodId,
                ReferenceNumber = $"SALE-{sale.Id}",
                SaleId = sale.Id,
                TenantId = tenantId,
            };

            _context.StoreAccounts.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<(decimal TotalIncome, decimal TotalExpenses, decimal TotalCapital, decimal CashBalance)> GetSummaryDataAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return (0m, 0m, 0m, 0m);
            
            // حساب قيمة المنتجات في المخزون (فقط للمستخدم الحالي)
            var productsValue = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .SumAsync(p => p.PurchasePrice * p.Quantity);
                
            // حساب الدخل والمصروفات (يستخدم بالفعل UserId من خلال GetTotalIncomeAsync و GetTotalExpensesAsync)
            var totalIncome = await GetTotalIncomeAsync();
            var totalExpenses = await GetTotalExpensesAsync();
            var cashBalance = Math.Max(0, totalIncome - totalExpenses); // لا نجعل الكاش بالسالب
            
            // رأس المال = قيمة المنتجات + الرصيد النقدي
            var totalCapital = productsValue + cashBalance;
            return (totalIncome, totalExpenses, totalCapital, cashBalance);
        }

        public async Task AutoCreateTransactionFromGeneralDebtAsync(GeneralDebt debt, StoreAccountViewModel model)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            var userId = await _userService.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("المستخدم غير مسجل دخول");

            var transactionType = debt.DebtType == GeneralDebtType.OwedToMe ? TransactionType.Income : TransactionType.Expense;
            var transaction = new StoreAccount
            {
                TransactionName = model.TransactionName,
                TransactionType = transactionType,
                Amount = model.Amount,
                TransactionDate = model.TransactionDate,
                Description = model.Description,
                Category = model.Category,
                PaymentMethodId = model.PaymentMethodId,
                ReferenceNumber = model.ReferenceNumber,
                Notes = model.Notes,
                GeneralDebtId = debt.Id,
                TenantId = tenantId,
            };
            _context.StoreAccounts.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<(decimal Receivables, decimal Payables, decimal Net)> GetGeneralDebtsSummaryAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return (0m, 0m, 0m);

            var receivables = await _context.GeneralDebts
                .Where(d => d.DebtType == GeneralDebtType.OwedToMe && d.TenantId == tenantId)
                .SumAsync(d => d.Amount - d.PaidAmount);
                
            var payables = await _context.GeneralDebts
                .Where(d => d.DebtType == GeneralDebtType.OnMe && d.TenantId == tenantId)
                .SumAsync(d => d.Amount - d.PaidAmount);
                
            var net = receivables - payables;
            return (receivables, payables, net);
        }
    }
} 
