 
using ManageMentSystem.Models;
using ManageMentSystem.Helpers;
using ManageMentSystem.Services.GeneralDebtServices;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class GeneralDebtsController : Controller
    {
        private readonly IGeneralDebtService _service;

        public GeneralDebtsController(IGeneralDebtService service)
        {
            _service = service;
        }



        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var allDebts = await _service.GetAllAsync();

            var filtered = allDebts
                .ToList();

            var totalItems = filtered.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var debts = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new GeneralDebtIndexViewModel
            {
                Debts = debts.Select(d => new GeneralDebtListItemViewModel
                {
                    Id = d.Id,
                    Title = d.Title,
                    PartyName = d.PartyName,
                    DebtType = d.DebtType,
                    Amount = d.Amount,
                    PaidAmount = d.PaidAmount,
                    CreatedAt = d.CreatedAt,
                    DueDate = d.DueDate,
                    Description = d.Description
                }).ToList(),
                TotalReceivables = debts.Where(d => d.DebtType == GeneralDebtType.OwedToMe).Sum(d => d.Amount - d.PaidAmount),
                TotalPayables = debts.Where(d => d.DebtType == GeneralDebtType.OnMe).Sum(d => d.Amount - d.PaidAmount),
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalItems = totalItems
            };
            ViewBag.PaymentMethods = await _service.GetPaymentMethodsAsync();
            return View(vm);
        }

        public IActionResult Create()
        {
            ViewBag.PaymentMethods = _service.GetPaymentMethodsAsync().Result;
            return View(new CreateGeneralDebtViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGeneralDebtViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PaymentMethods = await _service.GetPaymentMethodsAsync();
                return View(model);
            }

            try
            {
                var (debt, infoMessage) = await _service.CreateAsync(model);

                if (!string.IsNullOrEmpty(infoMessage))
                {
                    TempData["Info"] = infoMessage;
                }
                else
                {
                    TempData["Success"] = "تم إنشاء الدين بنجاح";
                }
            }
            catch (InvalidOperationException ex)
            {
                // خطأ رصيد غير كافٍ لوسيلة الدفع
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.PaymentMethods = await _service.GetPaymentMethodsAsync();
                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "حدث خطأ أثناء إنشاء الدين";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var debt = await _service.GetByIdAsync(id);
            if (debt == null) return NotFound();
            var vm = new CreateGeneralDebtViewModel
            {
                Id = debt.Id,
                Title = debt.Title,
                PartyName = debt.PartyName,
                DebtType = debt.DebtType,
                Amount = debt.Amount,
                PaidAmount = debt.PaidAmount,
                DueDate = debt.DueDate,
                Description = debt.Description
            };
            ViewBag.PaymentMethods = await _service.GetPaymentMethodsAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateGeneralDebtViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            await _service.UpdateAsync(id, model);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(int id, decimal amount, int? paymentMethodId, string? description)
        {
            try
            {
                var (residual, warningMessage) = await _service.AddPaymentAsync(id, amount, paymentMethodId, description);

                if (!string.IsNullOrEmpty(warningMessage))
                {
                    // لو فيه warning أو error
                    if (residual == 0 && (warningMessage.Contains("أكبر من المبلغ المتبقي") || warningMessage.Contains("تم سداده بالكامل")))
                    {
                        TempData["Error"] = warningMessage;
                    }
                    else
                    {
                        TempData["Info"] = warningMessage;
                    }
                }
                else if (residual > 0)
                {
                    TempData["Info"] = $"تم خصم الكاش المتاح، وتم تحويل {residual:C} كدين جديد على المحل";
                }
                else
                {
                    TempData["Success"] = "تمت العملية بنجاح";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] = "حدث خطأ أثناء معالجة العملية";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}