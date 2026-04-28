using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using ManageMentSystem.Data;
using ManageMentSystem.Helpers;
using ManageMentSystem.Models;
using ManageMentSystem.Services.StoreAccountServices;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class StoreAccountController : Controller
    {
        private readonly IStoreAccountService _storeAccountService;
        private readonly IUserService _userService;
        private readonly AppDbContext _context;

        public StoreAccountController(IStoreAccountService storeAccountService, AppDbContext context, IUserService userService)
        {
            _storeAccountService = storeAccountService;
            _context = context;
            _userService = userService;
        }

        private async Task<SelectList> GetPaymentMethodsSelectListAsync()
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return new SelectList(new List<object>(), "Value", "Text");
            return new SelectList(await _context.PaymentMethodOptions
                .Where(pm => pm.IsActive && pm.TenantId == tenantId)
                .OrderBy(pm => pm.SortOrder)
                .Select(pm => new { Value = pm.Id, Text = pm.Name })
                .ToListAsync(), "Value", "Text");
        }

        public async Task<IActionResult> Index(StoreAccountFilterViewModel? filter = null, int page = 1, int pageSize = 20)
        {
            if (filter == null)
            {
                filter = new StoreAccountFilterViewModel
                {
                    FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    ToDate = DateTime.Today.AddDays(1).AddTicks(-1)
                };
            }
            else
            {
                if (!filter.FromDate.HasValue)
                    filter.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                if (!filter.ToDate.HasValue)
                    filter.ToDate = DateTime.Today.AddDays(1).AddTicks(-1);
                else
                    filter.ToDate = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            ViewBag.TransactionTypes = new SelectList(Enum.GetValues(typeof(ManageMentSystem.Models.TransactionType))
                .Cast<ManageMentSystem.Models.TransactionType>()
                .Select(t => new { Value = t, Text = t.GetDisplayName() }), "Value", "Text");

            ViewBag.PaymentMethods = await GetPaymentMethodsSelectListAsync();

            var allTransactions = filter != null
                ? await _storeAccountService.GetFilteredTransactionsAsync(filter)
                : await _storeAccountService.GetAllTransactionsAsync();

            var totalItems = allTransactions.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedTransactions = allTransactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var (totalIncome, totalExpenses, totalCapital, cashBalance) = await _storeAccountService.GetSummaryDataAsync();
            var (gdReceivables, gdPayables, gdNet) = await _storeAccountService.GetGeneralDebtsSummaryAsync();
            ViewBag.TotalIncome = totalIncome;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.NetProfit = totalIncome - totalExpenses;
            ViewBag.TotalCapital = totalCapital;
            ViewBag.CashBalance = cashBalance;
            ViewBag.GDReceivables = gdReceivables;
            ViewBag.GDPayables = gdPayables;
            ViewBag.GDNet = gdNet;

            ViewBag.Filter = filter ?? new StoreAccountFilterViewModel();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(pagedTransactions);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.TransactionTypes = new SelectList(Enum.GetValues(typeof(ManageMentSystem.Models.TransactionType))
                .Cast<ManageMentSystem.Models.TransactionType>()
                .Select(t => new { Value = t, Text = t.GetDisplayName() }), "Value", "Text");

            ViewBag.PaymentMethods = await GetPaymentMethodsSelectListAsync();

            return View(new StoreAccountViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoreAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _storeAccountService.CreateTransactionAsync(model);
                    TempData["Success"] = "تم إضافة العملية بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"حدث خطأ: {ex.Message}");
                }
            }

            ViewBag.TransactionTypes = new SelectList(Enum.GetValues(typeof(ManageMentSystem.Models.TransactionType))
                .Cast<ManageMentSystem.Models.TransactionType>()
                .Select(t => new { Value = t, Text = t.GetDisplayName() }), "Value", "Text");

            ViewBag.PaymentMethods = new SelectList(_context.PaymentMethodOptions
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.SortOrder)
                .Select(pm => new { Value = pm.Id, Text = pm.Name }), "Value", "Text");

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _storeAccountService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            var model = new StoreAccountViewModel
            {
                Id = transaction.Id,
                TransactionName = transaction.TransactionName,
                TransactionType = transaction.TransactionType,
                Amount = transaction.Amount,
                TransactionDate = transaction.TransactionDate,
                Description = transaction.Description,
                Category = transaction.Category,
                PaymentMethodId = transaction.PaymentMethodId,
                ReferenceNumber = transaction.ReferenceNumber,
                Notes = transaction.Notes,
                SaleId = transaction.SaleId
            };

            ViewBag.TransactionTypes = new SelectList(Enum.GetValues(typeof(ManageMentSystem.Models.TransactionType))
                .Cast<ManageMentSystem.Models.TransactionType>()
                .Select(t => new { Value = t, Text = t.GetDisplayName() }), "Value", "Text");

            ViewBag.PaymentMethods = new SelectList(_context.PaymentMethodOptions
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.SortOrder)
                .Select(pm => new { Value = pm.Id, Text = pm.Name }), "Value", "Text");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StoreAccountViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _storeAccountService.UpdateTransactionAsync(id, model);
                    TempData["Success"] = "تم تحديث العملية بنجاح";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"حدث خطأ: {ex.Message}");
                }
            }

            ViewBag.TransactionTypes = new SelectList(Enum.GetValues(typeof(ManageMentSystem.Models.TransactionType))
                .Cast<ManageMentSystem.Models.TransactionType>()
                .Select(t => new { Value = t, Text = t.GetDisplayName() }), "Value", "Text");

            ViewBag.PaymentMethods = new SelectList(_context.PaymentMethodOptions
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.SortOrder)
                .Select(pm => new { Value = pm.Id, Text = pm.Name }), "Value", "Text");

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _storeAccountService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _storeAccountService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _storeAccountService.DeleteTransactionAsync(id);
                TempData["Success"] = "تم حذف العملية بنجاح";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث خطأ: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                var balance = await _storeAccountService.GetTotalCapitalAsync();
                return Json(new { success = true, balance = balance });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBalanceByMethod(int? paymentMethodId)
        {
            try
            {
                var cashBalance = await _storeAccountService.GetCashBalanceByPaymentMethodAsync(paymentMethodId);
                return Json(new { success = true, balance = cashBalance });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> RealizedProfit(DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20)
        {
            if (!fromDate.HasValue)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!toDate.HasValue)
                toDate = DateTime.Today.AddDays(1).AddTicks(-1);

            if (toDate.HasValue && toDate.Value.TimeOfDay == TimeSpan.Zero)
            {
                toDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) tenantId = string.Empty;
            var customerPaymentsQuery = _context.CustomerPayments
                .Where(cp => cp.Customer.TenantId == tenantId)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                var fromDateStart = fromDate.Value.Date;
                customerPaymentsQuery = customerPaymentsQuery.Where(cp => cp.PaymentDate >= fromDateStart);
            }

            if (toDate.HasValue)
            {
                var toDateEnd = toDate.Value.Date == toDate.Value ?
                    toDate.Value.AddDays(1).AddTicks(-1) : toDate.Value;
                customerPaymentsQuery = customerPaymentsQuery.Where(cp => cp.PaymentDate <= toDateEnd);
            }

            var customerPayments = await customerPaymentsQuery
                .Include(cp => cp.Customer)
                .Include(cp => cp.Allocations)
                .ThenInclude(a => a.Sale)
                .ThenInclude(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .OrderByDescending(cp => cp.PaymentDate)
                .ToListAsync();

            var totalPaidAmount = customerPayments.Sum(cp => cp.Amount);
            var totalRealizedProfit = 0m;

            foreach (var customerPayment in customerPayments)
            {
                var realizedProfitFromPayment = 0m;
                var totalAllocatedAmount = 0m;
                var paymentSign = customerPayment.Amount < 0 ? -1m : 1m;

                foreach (var allocation in customerPayment.Allocations)
                {
                    var sale = allocation.Sale;
                    if (sale != null && sale.SaleItems.Any())
                    {
                        var totalCostOfGoods = sale.SaleItems
                            .Where(si => si.Product != null)
                            .Sum(si => si.Product.PurchasePrice * si.Quantity);

                        var grossAmountAfterDiscount = sale.TotalAmount;
                        var netSaleAmount = Math.Max(0, grossAmountAfterDiscount - sale.ReturnedAmount);

                        decimal adjustedCostOfGoods = totalCostOfGoods;
                        if (grossAmountAfterDiscount > 0 && netSaleAmount < grossAmountAfterDiscount)
                        {
                            var keptRatio = netSaleAmount / grossAmountAfterDiscount;
                            adjustedCostOfGoods = totalCostOfGoods * keptRatio;
                        }

                        var profitMargin = netSaleAmount > 0 ? (netSaleAmount - adjustedCostOfGoods) / netSaleAmount : 0;
                        var profitFromAllocation = allocation.Amount * profitMargin * paymentSign;
                        realizedProfitFromPayment += profitFromAllocation;
                        totalAllocatedAmount += allocation.Amount;
                    }
                }

                totalRealizedProfit += realizedProfitFromPayment;

                var profitMarginPercentage = totalAllocatedAmount > 0 ? (realizedProfitFromPayment / totalAllocatedAmount) * 100 : 0;
                customerPayment.Notes = $"الربح المحقق: {realizedProfitFromPayment:C} (نسبة الربح: {profitMarginPercentage:F1}%)";
            }

            var netRealizedProfit = totalRealizedProfit;
            var totalItems = customerPayments.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedTransactions = customerPayments
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPaidAmount = totalPaidAmount;
            ViewBag.TotalRealizedProfit = totalRealizedProfit;
            ViewBag.NetRealizedProfit = netRealizedProfit;
            ViewBag.ProfitPercentage = totalPaidAmount > 0 ? (netRealizedProfit / totalPaidAmount) * 100 : 0;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(pagedTransactions);
        }

        public async Task<IActionResult> Export(StoreAccountFilterViewModel filter)
        {
            var transactions = await _storeAccountService.GetFilteredTransactionsAsync(filter);

            return Json(new {
                success = true,
                data = transactions.Select(t => new {
                    t.Id,
                    t.TransactionName,
                    TransactionType = t.TransactionType.GetDisplayName(),
                    t.Amount,
                    TransactionDate = t.TransactionDate.ToString("yyyy-MM-dd"),
                    t.Description,
                    t.Category,
                    PaymentMethod = t.PaymentMethod.Name,
                    t.ReferenceNumber
                })
            });
        }

        public async Task<IActionResult> InstallmentDetails(DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            if (!fromDate.HasValue)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!toDate.HasValue)
                toDate = DateTime.Today.AddDays(1).AddTicks(-1);

            var downPaymentsQuery = _context.StoreAccounts
                .Where(sa => sa.Category == "الأقساط" &&
                             sa.ReferenceNumber.StartsWith("INST-DOWN") &&
                             sa.TenantId == tenantId);

            var installmentPaymentsQuery = _context.StoreAccounts
                .Where(sa => sa.Category == "الأقساط" &&
                             sa.ReferenceNumber.StartsWith("INST-PAYMENT") &&
                             sa.TenantId == tenantId);

            downPaymentsQuery = downPaymentsQuery.Where(sa => sa.TransactionDate >= fromDate.Value && sa.TransactionDate <= toDate.Value);
            installmentPaymentsQuery = installmentPaymentsQuery.Where(sa => sa.TransactionDate >= fromDate.Value && sa.TransactionDate <= toDate.Value);

            var downPayments = await downPaymentsQuery.OrderByDescending(sa => sa.TransactionDate).ToListAsync();
            var installmentPayments = await installmentPaymentsQuery.OrderByDescending(sa => sa.TransactionDate).ToListAsync();

            var allInstallmentTransactions = new List<(StoreAccount Transaction, decimal Profit)>();

            var installmentIds = downPayments
                .Select(dp => dp.ReferenceNumber.Replace("INST-DOWN-", "").Trim())
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();

            if (installmentIds.Any())
            {
                var installmentsWithProducts = await _context.Installments
                    .Include(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                    .Where(i => installmentIds.Contains(i.Id) && i.TenantId == tenantId)
                    .ToListAsync();

                foreach (var downPayment in downPayments)
                {
                    if (int.TryParse(downPayment.ReferenceNumber.Replace("INST-DOWN-", "").Trim(), out int instId))
                    {
                        var installment = installmentsWithProducts.FirstOrDefault(i => i.Id == instId);
                        if (installment?.InstallmentItems?.Any() == true)
                        {
                            var totalPurchasePrice = installment.InstallmentItems.Sum(ii => (ii.Product?.PurchasePrice ?? 0) * ii.Quantity);
                            var totalAmountWithInterest = installment.TotalWithInterest + installment.ExtraMonthAmount;
                            var installmentTotalProfit = totalAmountWithInterest - totalPurchasePrice;
                            var profitRatio = totalAmountWithInterest > 0 ? installmentTotalProfit / totalAmountWithInterest : 0;
                            var profit = downPayment.Amount * profitRatio;

                            downPayment.Notes = $"الربح: {profit:N2} (من أصل {downPayment.Amount:N2}) - إجمالي العقد: {totalAmountWithInterest:N2}";
                            allInstallmentTransactions.Add((downPayment, profit));
                        }
                        else { allInstallmentTransactions.Add((downPayment, 0m)); }
                    }
                }
            }

            var paymentIds = installmentPayments
                .Select(ip => ip.ReferenceNumber.Replace("INST-PAYMENT-", "").Trim())
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();

            if (paymentIds.Any())
            {
                var paymentsWithDetails = await _context.InstallmentPayments
                    .Include(ip => ip.Installment)
                    .ThenInclude(i => i.InstallmentItems)
                    .ThenInclude(ii => ii.Product)
                    .Where(ip => paymentIds.Contains(ip.Id) && ip.Installment.TenantId == tenantId)
                    .ToListAsync();

                foreach (var payment in installmentPayments)
                {
                    if (int.TryParse(payment.ReferenceNumber.Replace("INST-PAYMENT-", "").Trim(), out int payId))
                    {
                        var paymentDetail = paymentsWithDetails.FirstOrDefault(p => p.Id == payId);
                        if (paymentDetail?.Installment?.InstallmentItems?.Any() == true)
                        {
                            var totalPurchasePrice = paymentDetail.Installment.InstallmentItems.Sum(ii => (ii.Product?.PurchasePrice ?? 0) * ii.Quantity);
                            var totalAmountWithInterest = paymentDetail.Installment.TotalWithInterest + paymentDetail.Installment.ExtraMonthAmount;
                            var paymentTotalProfit = totalAmountWithInterest - totalPurchasePrice;
                            var profitRatio = totalAmountWithInterest > 0 ? paymentTotalProfit / totalAmountWithInterest : 0;
                            var profit = payment.Amount * profitRatio;

                            payment.Notes = $"الربح: {profit:N2} (من قيمة قسط {payment.Amount:N2}) - إجمالي العقد: {totalAmountWithInterest:N2}";
                            allInstallmentTransactions.Add((payment, profit));
                        }
                        else { allInstallmentTransactions.Add((payment, 0m)); }
                    }
                }
            }

            var sortedTransactions = allInstallmentTransactions
                .OrderByDescending(t => t.Transaction.TransactionDate)
                .ToList();

            var pagedTransactions = sortedTransactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalDownPayments = downPayments.Sum(sa => sa.Amount);
            ViewBag.TotalInstallmentPayments = installmentPayments.Sum(sa => sa.Amount);
            ViewBag.TotalInstallments = (decimal)ViewBag.TotalDownPayments + (decimal)ViewBag.TotalInstallmentPayments;
            ViewBag.TotalProfit = sortedTransactions.Sum(t => t.Profit);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)sortedTransactions.Count / pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = sortedTransactions.Count;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(pagedTransactions.Select(t => t.Transaction).ToList());
        }

        public async Task<IActionResult> PaymentMethodBalances(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            if (toDate.HasValue && toDate.Value.TimeOfDay == TimeSpan.Zero)
            {
                toDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            var query = _context.StoreAccounts
                .Where(sa => sa.TenantId == tenantId)
                .Include(sa => sa.PaymentMethod)
                .AsNoTracking()
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(sa => sa.TransactionDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(sa => sa.TransactionDate <= toDate.Value);

            var groups = await query
                .GroupBy(sa => new { sa.PaymentMethodId, sa.TenantId })
                .Select(g => new
                {
                    PaymentMethodId = g.Key.PaymentMethodId,
                    Name = g.Max(x => x.PaymentMethod != null ? x.PaymentMethod.Name : "غير محدد"),
                    Income = g.Where(x => x.TransactionType == ManageMentSystem.Models.TransactionType.Income).Sum(x => x.Amount),
                    Expenses = g.Where(x => x.TransactionType == ManageMentSystem.Models.TransactionType.Expense).Sum(x => x.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Income - x.Expenses)
                .ToListAsync();

            var model = groups.Select(g => new ManageMentSystem.ViewModels.PaymentMethodBalanceViewModel
            {
                PaymentMethodId = g.PaymentMethodId,
                Name = g.Name,
                TotalIncome = g.Income,
                TotalExpenses = g.Expenses,
                TransactionCount = g.Count
            }).ToList();

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(model);
        }

        public async Task<IActionResult> PaymentMethodDetails(int? id, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20)
        {
            if (!id.HasValue) return NotFound();

            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            if (!fromDate.HasValue && !toDate.HasValue)
            {
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                toDate = DateTime.Today.AddDays(1).AddTicks(-1);
            }

            if (toDate.HasValue && toDate.Value.TimeOfDay == TimeSpan.Zero)
            {
                toDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            var query = _context.StoreAccounts
                .Include(sa => sa.PaymentMethod)
                .Include(sa => sa.Sale).ThenInclude(s => s.Customer)
                .Include(sa => sa.GeneralDebt)
                .Where(sa => sa.PaymentMethodId == id && sa.TenantId == tenantId)
                .AsNoTracking()
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(sa => sa.TransactionDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(sa => sa.TransactionDate <= toDate.Value);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await query
                .OrderByDescending(sa => sa.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var methodName = await _context.PaymentMethodOptions
                .Where(pm => pm.Id == id.Value && pm.TenantId == tenantId)
                .Select(pm => pm.Name)
                .FirstOrDefaultAsync() ?? "غير محدد";

            ViewBag.PaymentMethodId = id.Value;
            ViewBag.PaymentMethodName = methodName;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(items);
        }
    }
}
