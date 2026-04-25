 
using ManageMentSystem.Data;
using ManageMentSystem.Models;
using ManageMentSystem.Helpers;
using ManageMentSystem.Services.PaymentOptionServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class PaymentMethodsController : Controller
    {
        private readonly IPaymentOptionService _paymentservice;
        private readonly AppDbContext _context;

        public PaymentMethodsController(IPaymentOptionService paymentservice ,AppDbContext context)
        {
            _paymentservice = paymentservice;
            _context = context;
        }

        // GET: Settings/PaymentMethods
        public async Task<IActionResult> Index()
        {
            var methods = await _paymentservice.GetAllAsync();
            return View(methods);
        }

        // GET: Settings/PaymentMethods/Create
        public IActionResult Create()
        {
            return View(new PaymentMethodOption());
        }

        // POST: Settings/PaymentMethods/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentMethodOption model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.IsDefault)
            {
                var all = await _paymentservice.GetAllAsync();
                foreach (var m in all)
                {
                    m.IsDefault = false;
                }
            }
            
            _paymentservice.CreateAsync(model);
            TempData["SuccessMessage"] = "تمت إضافة طريقة الدفع.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Settings/PaymentMethods/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var method = await _paymentservice.GetByIdAsync(id);
            if (method == null) return NotFound();
            return View(method);
        }

        // POST: Settings/PaymentMethods/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentMethodOption model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var method = await _paymentservice.GetByIdAsync(id);
            if (method == null) return NotFound();

            method.Name = model.Name;
            method.IsActive = model.IsActive;
            method.SortOrder = model.SortOrder;

            _paymentservice.UpdateAsync(method);

            if (model.IsDefault && !method.IsDefault)
            {
                var all = await _paymentservice.GetAllAsync();
                foreach (var m in all)
                {
                    m.IsDefault = false;
                }
                method.IsDefault = true;
            }
            else if (!model.IsDefault && method.IsDefault)
            {
                method.IsDefault = false;
            }
            TempData["SuccessMessage"] = "تم تحديث طريقة الدفع.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Settings/PaymentMethods/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var method = await _paymentservice.GetByIdAsync(id);
            if (method == null) return NotFound();
            return View(method);
        }

        // POST: Settings/PaymentMethods/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var method = await _paymentservice.GetByIdAsync(id);
            if (method == null) return NotFound();

            // Prevent deleting if referenced (optional safeguard)
            var inUse = await _context.StoreAccounts.AnyAsync(sa => sa.PaymentMethodId == id);
            if (inUse)
            {
                TempData["ErrorMessage"] = "لا يمكن حذف طريقة دفع مستخدمة في معاملات.";
                return RedirectToAction(nameof(Index));
            }

            _paymentservice.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم حذف طريقة الدفع.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Settings/PaymentMethods/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var method = await _context.PaymentMethodOptions.FindAsync(id);
            if (method == null) return NotFound();
            method.IsActive = !method.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Settings/PaymentMethods/SetDefault/5
        [HttpPost]
        public async Task<IActionResult> SetDefault(int id)
        {
            var all = await _context.PaymentMethodOptions.ToListAsync();
            foreach (var m in all) m.IsDefault = false;

            var method = all.FirstOrDefault(m => m.Id == id);
            if (method == null) return NotFound();
            method.IsDefault = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

