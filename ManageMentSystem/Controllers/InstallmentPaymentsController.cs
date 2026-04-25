using ManageMentSystem.Models;
using ManageMentSystem.Helpers;
using ManageMentSystem.Services.InstallmentServices;
using ManageMentSystem.Services.PaymentOptionServices;
using ManageMentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageMentSystem.Controllers
{
    [Authorize]
    public class InstallmentPaymentsController : Controller
    {
        private readonly IInstallmentPaymentService _paymentService;
        private readonly IInstallmentService _installmentService;
        private readonly IPaymentOptionService _paymentOption;

        public InstallmentPaymentsController(IInstallmentPaymentService paymentService, IInstallmentService installmentService , IPaymentOptionService paymentOption)
        {
            _paymentService = paymentService;
            _installmentService = installmentService;
            _paymentOption = paymentOption;
        }

        // GET: InstallmentPayments
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            // إذا لم يوجد View باسم Index في مجلد InstallmentPayments، أعد التوجيه إلى صفحة الأقساط
            return RedirectToAction("Index", "Installments");
        }

        // GET: InstallmentPayments/Create
        public async Task<IActionResult> Create(int? installmentId = null)
        {
            if (installmentId.HasValue)
            {
                var installmentDetails = await _paymentService.GetInstallmentDetailsForPaymentAsync(installmentId.Value);
                if (installmentDetails == null)
                    return NotFound();

                ViewBag.PaymentMethods = await _paymentOption.GetActiveAsync();


                return View(installmentDetails);
            }

            // If no installment specified, show list of active installments
            var activeInstallments = await _installmentService.GetAllInstallmentsAsync();
            ViewBag.ActiveInstallments = activeInstallments.Where(i => i.Status == "نشط" || i.Status == "متأخر").ToList();

            return View("SelectInstallment");
        }

        // POST: InstallmentPayments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInstallmentPaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var payment = await _paymentService.AddPaymentAsync(model);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "تم إضافة الدفعة بنجاح" });
                    }

                    return RedirectToAction("Details", "Installments", new { id = model.InstallmentId, message = "تم إضافة الدفعة بنجاح" });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ في البيانات",
                    errors = errors
                });
            }

            return View(model);
        }
      
    }
}