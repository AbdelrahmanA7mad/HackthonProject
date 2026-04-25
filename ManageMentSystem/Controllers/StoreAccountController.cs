 
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


        public StoreAccountController(IStoreAccountService storeAccountService, AppDbContext context , IUserService userService)
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


        // GET: StoreAccount
        public async Task<IActionResult> Index(StoreAccountFilterViewModel? filter = null, int page = 1, int pageSize = 20)
        {
            // ØªØ¹ÙŠÙŠÙ† Ø§Ù„ØªÙˆØ§Ø±ÙŠØ® Ø§Ù„Ø§ÙØªØ±Ø§Ø¶ÙŠØ© Ø¥Ø°Ø§ Ù„Ù… ÙŠØªÙ… ØªØ­Ø¯ÙŠØ¯Ù‡Ø§
            if (filter == null)
            {
                filter = new StoreAccountFilterViewModel
                {
                    FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    ToDate = DateTime.Today.AddDays(1).AddTicks(-1) // Ø¢Ø®Ø± Ø«Ø§Ù†ÙŠØ© ÙÙŠ Ø§Ù„ÙŠÙˆÙ…
                };
            }
            else
            {
                if (!filter.FromDate.HasValue)
                    filter.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                if (!filter.ToDate.HasValue)
                    filter.ToDate = DateTime.Today.AddDays(1).AddTicks(-1);
                else
                    // ØªØ¹Ø¯ÙŠÙ„ ToDate Ù„Ùˆ ÙƒØ§Ù† Ø§Ù„ØªØ§Ø±ÙŠØ® Ø¨Ø¯ÙˆÙ† ÙˆÙ‚Øª
                    filter.ToDate = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            // ØªØ­Ù…ÙŠÙ„ Ø§Ù„Ù‚ÙˆØ§Ø¦Ù…
            ViewBag.TransactionTypes = new SelectList(Enum.GetValues(typeof(ManageMentSystem.Models.TransactionType))
                .Cast<ManageMentSystem.Models.TransactionType>()
                .Select(t => new { Value = t, Text = t.GetDisplayName() }), "Value", "Text");

            ViewBag.PaymentMethods = await GetPaymentMethodsSelectListAsync();

            // Ø¬Ù„Ø¨ Ø§Ù„Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„Ù…ÙÙ„ØªØ±Ø©
            var allTransactions = filter != null
                ? await _storeAccountService.GetFilteredTransactionsAsync(filter)
                : await _storeAccountService.GetAllTransactionsAsync();

            // ØªØ·Ø¨ÙŠÙ‚ ØªØ±Ù‚ÙŠÙ… Ø§Ù„ØµÙØ­Ø§Øª
            var totalItems = allTransactions.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedTransactions = allTransactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Ø¬Ù„Ø¨ Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„Ù…Ù„Ø®Øµ
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

        // GET: StoreAccount/Create
                public async Task<IActionResult> Create()
        {
            ViewBag.TransactionTypes = new SelectList(Enum.GetValues(typeof(ManageMentSystem.Models.TransactionType))
                .Cast<ManageMentSystem.Models.TransactionType>()
                .Select(t => new { Value = t, Text = t.GetDisplayName() }), "Value", "Text");

            ViewBag.PaymentMethods = await GetPaymentMethodsSelectListAsync();

            return View(new StoreAccountViewModel());
        }

        // POST: StoreAccount/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
                public async Task<IActionResult> Create(StoreAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _storeAccountService.CreateTransactionAsync(model);
                    TempData["Success"] = "ØªÙ… Ø¥Ø¶Ø§ÙØ© Ø§Ù„Ø¹Ù…Ù„ÙŠØ© Ø¨Ù†Ø¬Ø§Ø­";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ø­Ø¯Ø« Ø®Ø·Ø£: {ex.Message}");
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

        // GET: StoreAccount/Edit/5

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

        // POST: StoreAccount/Edit/5
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
                    TempData["Success"] = "ØªÙ… ØªØ­Ø¯ÙŠØ« Ø§Ù„Ø¹Ù…Ù„ÙŠØ© Ø¨Ù†Ø¬Ø§Ø­";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ø­Ø¯Ø« Ø®Ø·Ø£: {ex.Message}");
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

        // GET: StoreAccount/Details/5
                public async Task<IActionResult> Details(int id)
        {
            var transaction = await _storeAccountService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: StoreAccount/Delete/5

                public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _storeAccountService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: StoreAccount/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
                public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _storeAccountService.DeleteTransactionAsync(id);
                TempData["Success"] = "ØªÙ… Ø­Ø°Ù Ø§Ù„Ø¹Ù…Ù„ÙŠØ© Ø¨Ù†Ø¬Ø§Ø­";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ø­Ø¯Ø« Ø®Ø·Ø£: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: StoreAccount/GetBalance
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

        // GET: StoreAccount/GetBalanceByMethod
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
        // GET: StoreAccount/RealizedProfit
                public async Task<IActionResult> RealizedProfit(DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20)
        {
            if (!fromDate.HasValue)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!toDate.HasValue)
                toDate = DateTime.Today.AddDays(1).AddTicks(-1);

            // ØªØ£ÙƒØ¯ Ù…Ù† Ø£Ù† toDate ØªØ´Ù…Ù„ Ø§Ù„ÙŠÙˆÙ… ÙƒØ§Ù…Ù„
            if (toDate.HasValue && toDate.Value.TimeOfDay == TimeSpan.Zero)
            {
                toDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            // Ø¬Ù„Ø¨ Ø¯ÙØ¹Ø§Øª Ø§Ù„Ø¹Ù…Ù„Ø§Ø¡ Ø§Ù„Ø®Ø§ØµØ© Ø¨Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… Ø§Ù„Ø­Ø§Ù„ÙŠ ÙÙ‚Ø·
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) tenantId = string.Empty;
            var customerPaymentsQuery = _context.CustomerPayments
                .Where(cp => cp.Customer.TenantId == tenantId)
                .AsQueryable();

            // ÙÙ„ØªØ±Ø© Ø§Ù„ØªÙˆØ§Ø±ÙŠØ®
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

            // Ø¥Ø¶Ø§ÙØ© Include Ø¨Ø¹Ø¯ Ø§Ù„ÙÙ„ØªØ±Ø©
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

            // Ø­Ø³Ø§Ø¨ Ø§Ù„Ø±Ø¨Ø­ Ø§Ù„Ù…Ø­Ù‚Ù‚ Ù„ÙƒÙ„ Ø¯ÙØ¹Ø© Ø¹Ù…ÙŠÙ„
            foreach (var customerPayment in customerPayments)
            {
                var realizedProfitFromPayment = 0m;
                var totalAllocatedAmount = 0m;

                // Ø­Ø³Ø§Ø¨ Ø§Ù„Ø±Ø¨Ø­ Ù…Ù† ÙƒÙ„ ØªØ®ØµÙŠØµ (allocation) ÙÙŠ Ù‡Ø°Ù‡ Ø§Ù„Ø¯ÙØ¹Ø©
                // ØªØ­Ø¯ÙŠØ¯ Ø¥Ø´Ø§Ø±Ø© Ø§Ù„Ø¯ÙØ¹Ø© (Ù…ÙˆØ¬Ø¨ Ù„Ù„Ø¯ÙØ¹Ø©ØŒ Ø³Ø§Ù„Ø¨ Ù„Ù„Ù…Ø±ØªØ¬Ø¹ ÙƒØ¯ÙØ¹Ø© Ø³Ø§Ù„Ø¨Ø©)
                var paymentSign = customerPayment.Amount < 0 ? -1m : 1m;

                foreach (var allocation in customerPayment.Allocations)
                {
                    var sale = allocation.Sale;
                    if (sale != null && sale.SaleItems.Any())
                    {
                        // Ø­Ø³Ø§Ø¨ Ø¥Ø¬Ù…Ø§Ù„ÙŠ ØªÙƒÙ„ÙØ© Ø§Ù„Ø¨Ø¶Ø§Ø¦Ø¹ Ø§Ù„Ù…Ø¨Ø§Ø¹Ø© ÙÙŠ Ù‡Ø°Ù‡ Ø§Ù„ÙØ§ØªÙˆØ±Ø©
                        var totalCostOfGoods = sale.SaleItems
                            .Where(si => si.Product != null)
                            .Sum(si => si.Product.PurchasePrice * si.Quantity);

                        // ØµØ§ÙÙŠ Ù‚ÙŠÙ…Ø© Ø§Ù„Ø¨ÙŠØ¹ Ø¨Ø¹Ø¯ Ø®ØµÙ… Ø§Ù„Ù…Ø±ØªØ¬Ø¹Ø§Øª (TotalAmount Ù…Ø®Ø²Ù† Ø¨Ø¹Ø¯ Ø§Ù„Ø®ØµÙ…)
                        var grossAmountAfterDiscount = sale.TotalAmount;
                        var netSaleAmount = Math.Max(0, grossAmountAfterDiscount - sale.ReturnedAmount);

                        // Ø¶Ø¨Ø· ØªÙƒÙ„ÙØ© Ø§Ù„Ø¨Ø¶Ø§Ø¦Ø¹ Ø¨Ù…Ø§ ÙŠØªÙ†Ø§Ø³Ø¨ Ù…Ø¹ Ù†Ø³Ø¨Ø© Ø§Ù„Ù…Ø±ØªØ¬Ø¹ (ØªÙ‚Ø±ÙŠØ¨ Ù…Ø¹Ù‚ÙˆÙ„ ÙÙŠ Ø­Ø§Ù„ Ø¹Ø¯Ù… ØªÙˆÙØ± ØªÙØ§ØµÙŠÙ„ Ø¹Ù†Ø§ØµØ± Ø§Ù„Ù…Ø±ØªØ¬Ø¹)
                        decimal adjustedCostOfGoods = totalCostOfGoods;
                        if (grossAmountAfterDiscount > 0 && netSaleAmount < grossAmountAfterDiscount)
                        {
                            var keptRatio = netSaleAmount / grossAmountAfterDiscount;
                            adjustedCostOfGoods = totalCostOfGoods * keptRatio;
                        }

                        // Ø­Ø³Ø§Ø¨ Ù†Ø³Ø¨Ø© Ø§Ù„Ø±Ø¨Ø­ Ù…Ù† ØµØ§ÙÙŠ Ø§Ù„ÙØ§ØªÙˆØ±Ø© Ø¨Ø¹Ø¯ Ø§Ù„Ù…Ø±ØªØ¬Ø¹
                        var profitMargin = netSaleAmount > 0 ? (netSaleAmount - adjustedCostOfGoods) / netSaleAmount : 0;

                        // Ø­Ø³Ø§Ø¨ Ø§Ù„Ø±Ø¨Ø­ Ø§Ù„Ù…Ø­Ù‚Ù‚ Ù…Ù† Ù‡Ø°Ø§ Ø§Ù„ØªØ®ØµÙŠØµ (Ø£Ø®Ø° Ø¥Ø´Ø§Ø±Ø© Ø§Ù„Ø¯ÙØ¹Ø© ÙÙŠ Ø§Ù„Ø§Ø¹ØªØ¨Ø§Ø±)
                        var profitFromAllocation = allocation.Amount * profitMargin * paymentSign;
                        realizedProfitFromPayment += profitFromAllocation;
                        totalAllocatedAmount += allocation.Amount;
                    }
                }

                totalRealizedProfit += realizedProfitFromPayment;

                // Ø¥Ø¶Ø§ÙØ© Ù…Ø¹Ù„ÙˆÙ…Ø§Øª Ø§Ù„Ø±Ø¨Ø­ ÙÙŠ Ø§Ù„Ù…Ù„Ø§Ø­Ø¸Ø§Øª
                var profitMarginPercentage = totalAllocatedAmount > 0 ? (realizedProfitFromPayment / totalAllocatedAmount) * 100 : 0;
                customerPayment.Notes = $"Ø§Ù„Ø±Ø¨Ø­ Ø§Ù„Ù…Ø­Ù‚Ù‚: {realizedProfitFromPayment:C} (Ù†Ø³Ø¨Ø© Ø§Ù„Ø±Ø¨Ø­: {profitMarginPercentage:F1}%)";
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

        // GET: StoreAccount/TempMoneyDetails

        // GET: StoreAccount/Export
                public async Task<IActionResult> Export(StoreAccountFilterViewModel filter)
        {
            var transactions = await _storeAccountService.GetFilteredTransactionsAsync(filter);
            
            // Here you can implement CSV or Excel export
            // For now, we'll return a JSON response
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
            // 1. Ø§Ù„ØªØ­Ù‚Ù‚ Ù…Ù† Ù‡ÙˆÙŠØ© Ø§Ù„Ù…Ø³ØªØ£Ø¬Ø±
            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            // 2. Ø¶Ø¨Ø· ØªÙˆØ§Ø±ÙŠØ® Ø§Ù„Ø¨Ø­Ø« Ø§Ù„Ø§ÙØªØ±Ø§Ø¶ÙŠØ©
            if (!fromDate.HasValue)
                fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!toDate.HasValue)
                toDate = DateTime.Today.AddDays(1).AddTicks(-1);

            // 3. ØªØ¬Ù‡ÙŠØ² Ø§Ù„Ø§Ø³ØªØ¹Ù„Ø§Ù…Ø§Øª Ø§Ù„Ø£Ø³Ø§Ø³ÙŠØ©
            var downPaymentsQuery = _context.StoreAccounts
                .Where(sa => sa.Category == "Ø§Ù„Ø£Ù‚Ø³Ø§Ø·" &&
                             sa.ReferenceNumber.StartsWith("INST-DOWN") &&
                             sa.TenantId == tenantId);

            var installmentPaymentsQuery = _context.StoreAccounts
                .Where(sa => sa.Category == "Ø§Ù„Ø£Ù‚Ø³Ø§Ø·" &&
                             sa.ReferenceNumber.StartsWith("INST-PAYMENT") &&
                             sa.TenantId == tenantId);

            // ÙÙ„ØªØ±Ø© Ø§Ù„ØªØ§Ø±ÙŠØ®
            downPaymentsQuery = downPaymentsQuery.Where(sa => sa.TransactionDate >= fromDate.Value && sa.TransactionDate <= toDate.Value);
            installmentPaymentsQuery = installmentPaymentsQuery.Where(sa => sa.TransactionDate >= fromDate.Value && sa.TransactionDate <= toDate.Value);

            var downPayments = await downPaymentsQuery.OrderByDescending(sa => sa.TransactionDate).ToListAsync();
            var installmentPayments = await installmentPaymentsQuery.OrderByDescending(sa => sa.TransactionDate).ToListAsync();

            var allInstallmentTransactions = new List<(StoreAccount Transaction, decimal Profit)>();

            // 4. Ù…Ø¹Ø§Ù„Ø¬Ø© Ù…Ù‚Ø¯Ù…Ø§Øª Ø§Ù„Ø£Ù‚Ø³Ø§Ø· (Down Payments)
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

                            // Ø§Ø³ØªØ®Ø¯Ø§Ù… Ù…Ø¹Ø§Ù…Ù„ Ø§Ù„Ø±Ø¨Ø­ Ù„Ø¶Ø±Ø¨ Ø§Ù„Ù‚ÙŠÙ…Ø© Ø§Ù„Ø­Ø§Ù„ÙŠØ©
                            var profitRatio = totalAmountWithInterest > 0 ? installmentTotalProfit / totalAmountWithInterest : 0;
                            var profit = downPayment.Amount * profitRatio;

                            // ØªØ­Ø³ÙŠÙ† ÙƒØªØ§Ø¨Ø© Ø§Ù„Ù…Ù„Ø§Ø­Ø¸Ø§Øª: Ø§Ø³ØªØ®Ø¯Ø§Ù… N2 Ø¨Ø¯Ù„Ø§Ù‹ Ù…Ù† C Ù„ØªØ¬Ù†Ø¨ Ù…Ø´Ø§ÙƒÙ„ Ø§Ù„Ø±Ù…ÙˆØ² Ø§Ù„Ø¹Ø±Ø¨ÙŠØ©
                            downPayment.Notes = $"Ø§Ù„Ø±Ø¨Ø­: {profit:N2} (Ù…Ù† Ø£ØµÙ„ {downPayment.Amount:N2}) - Ø¥Ø¬Ù…Ø§Ù„ÙŠ Ø§Ù„Ø¹Ù‚Ø¯: {totalAmountWithInterest:N2}";
                            allInstallmentTransactions.Add((downPayment, profit));
                        }
                        else { allInstallmentTransactions.Add((downPayment, 0m)); }
                    }
                }
            }

            // 5. Ù…Ø¹Ø§Ù„Ø¬Ø© Ø¯ÙØ¹Ø§Øª Ø§Ù„Ø£Ù‚Ø³Ø§Ø· (Installment Payments)
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

                            payment.Notes = $"Ø§Ù„Ø±Ø¨Ø­: {profit:N2} (Ù…Ù† Ù‚Ø³Ø· {payment.Amount:N2}) - Ø¥Ø¬Ù…Ø§Ù„ÙŠ Ø§Ù„Ø¹Ù‚Ø¯: {totalAmountWithInterest:N2}";
                            allInstallmentTransactions.Add((payment, profit));
                        }
                        else { allInstallmentTransactions.Add((payment, 0m)); }
                    }
                }
            }

            // 6. Ø§Ù„ØªØ±ØªÙŠØ¨ ÙˆØ§Ù„ØªØ±Ù‚ÙŠÙ… (Pagination)
            var sortedTransactions = allInstallmentTransactions
                .OrderByDescending(t => t.Transaction.TransactionDate)
                .ToList();

            var pagedTransactions = sortedTransactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 7. Ø¥Ø±Ø³Ø§Ù„ Ø§Ù„Ø¨ÙŠØ§Ù†Ø§Øª Ù„Ù„Ù€ View
            ViewBag.TotalDownPayments = downPayments.Sum(sa => sa.Amount);
            ViewBag.TotalInstallmentPayments = installmentPayments.Sum(sa => sa.Amount);
            ViewBag.TotalInstallments = (decimal)ViewBag.TotalDownPayments + (decimal)ViewBag.TotalInstallmentPayments;

            // Ø­Ø³Ø§Ø¨ Ø¥Ø¬Ù…Ø§Ù„ÙŠ Ø§Ù„Ø±Ø¨Ø­ Ù„ÙƒÙ„ Ø§Ù„Ø¹Ù…Ù„ÙŠØ§Øª Ø§Ù„Ù…Ø¹Ø±ÙˆØ¶Ø© (Ø£Ùˆ ÙƒÙ„ Ø§Ù„Ø¹Ù…Ù„ÙŠØ§Øª Ø­Ø³Ø¨ Ø­Ø§Ø¬ØªÙƒ)
            ViewBag.TotalProfit = sortedTransactions.Sum(t => t.Profit);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)sortedTransactions.Count / pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = sortedTransactions.Count;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(pagedTransactions.Select(t => t.Transaction).ToList());
        }


        // GET: StoreAccount/PaymentMethodBalances
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
                    Name = g.Max(x => x.PaymentMethod != null ? x.PaymentMethod.Name : "ØºÙŠØ± Ù…Ø­Ø¯Ø¯"),
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

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(model);
        }

        // GET: StoreAccount/PaymentMethodDetails/5
                public async Task<IActionResult> PaymentMethodDetails(int? id, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 20)
        {
            if (!id.HasValue) return NotFound();

            var tenantId = await _userService.GetCurrentTenantIdAsync();
            if (string.IsNullOrEmpty(tenantId)) return NotFound();

            // Ø§ÙØªØ±Ø§Ø¶ÙŠ: Ø§Ù„Ø´Ù‡Ø± Ø§Ù„Ø­Ø§Ù„ÙŠ
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
                .FirstOrDefaultAsync() ?? "ØºÙŠØ± Ù…Ø­Ø¯Ø¯";

            ViewBag.PaymentMethodId = id.Value;
            ViewBag.PaymentMethodName = methodName;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View(items);
        }






    }
} 
