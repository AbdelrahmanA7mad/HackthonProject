using DocumentFormat.OpenXml.Spreadsheet;
using ManageMentSystem.Helpers;
using ManageMentSystem.Models;
using ManageMentSystem.Services.UserInvoice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageMentSystem.Controllers
{

    [Authorize]

    public class UserInvoiceController : Controller
    {
        private readonly IUserInvoice _userInvoice;

        public UserInvoiceController(IUserInvoice userInvoice)
        {
            _userInvoice = userInvoice;
        }

                public async Task<IActionResult> Index()
        {
            var invoice = await _userInvoice.GetInvoiceAsync();

            // لو مفيش بيانات للمستخدم، نرجع كائن افتراضي
            if (invoice == null)
            {
                invoice = new Invoice
                {
                    CompanyName = "شركة غير محددة",
                    CompanySubtitle = "خدمات الفواتير",
                    PhoneNumbers = new List<string> { "01000000000" },
                    Address = "القاهرة، مصر",
                    FooterMessage = "شكرًا لتعاملكم معنا",
                    Logo = "/images/default-logo.png",
                    Website = "https://example.com",
                    Email = "info@example.com",
                };
            }

            return View(invoice);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
                public async Task<IActionResult> Edit(Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                // جمع كل أخطاء ModelState في نص واحد لعرضه (diagnostic)
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .Select(ms => new {
                        Key = ms.Key,
                        Errors = ms.Value.Errors.Select(e => e.ErrorMessage + " " + (e.Exception?.Message ?? "")).ToArray()
                    })
                    .ToList();

                // حوّل الأخطاء لسلسلة نصية قصيرة
                var errorText = string.Join(" | ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Errors)}"));

                // احتفظ بالأخطاء في TempData لعرضها في الـ View
                TempData["ErrorMessage"] = "Validation failed: " + errorText;

                return View("Index", invoice);
            }

            try
            {
                await _userInvoice.EditInvoiceAsync(invoice);
                TempData["SuccessMessage"] = "تم حفظ الإعدادات بنجاح ✅";
            }
            catch (Exception ex)
            {
                // لو في استثناء من قاعدة البيانات أو غيره، اعرضه برفق
                TempData["ErrorMessage"] = "حدث خطأ أثناء الحفظ: " + ex.Message;
            }

            return RedirectToAction("Index");
        }



    }
}

