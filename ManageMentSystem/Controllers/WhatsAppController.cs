// WhatsAppController.cs - محسّن
using ManageMentSystem.Helpers;
using ManageMentSystem.Models;
using ManageMentSystem.Services.UserServices;
using ManageMentSystem.Services.WhatsAppServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class WhatsAppController : Controller
{
    private readonly IUserService _user;
    private readonly IWhatsAppService _whatsAppService;

    public WhatsAppController(IUserService user, IWhatsAppService whatsAppService)
    {
        _user = user;
        _whatsAppService = whatsAppService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = await _user.GetRootUserIdAsync();
        var status = await _whatsAppService.GetSessionStatusAsync(userId);

        ViewBag.IsConnected = status.IsConnected;
        ViewBag.SessionExists = status.SessionExists;
        ViewBag.ServerError = status.ServerError;
        ViewBag.UserId = userId;
        ViewBag.UserName = User.Identity?.Name;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateSession()
    {
        var userId = await _user.GetRootUserIdAsync();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _whatsAppService.CreateSessionAsync(userId);

        if (result.Success)
        {
            return Json(new
            {
                success = true,
                message = "✅ تم إنشاء الجلسة بنجاح",
                needsQR = result.ResponseData?.needsQR
            });
        }

        return Json(new
        {
            success = false,
            message = $"❌ فشل في إنشاء الجلسة: {result.ErrorMessage}"
        });
    }

    [HttpPost]
    public async Task<IActionResult> DisconnectWhatsApp()
    {
        var userId = await _user.GetRootUserIdAsync();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _whatsAppService.DisconnectSessionAsync(userId);

        return Json(new
        {
            success = result.Success,
            message = result.Success ? "✅ تم قطع الاتصال بنجاح" : $"❌ فشل في قطع الاتصال: {result.ErrorMessage}"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetQRCode()
    {
        var userId = await _user.GetRootUserIdAsync();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _whatsAppService.GetQrCodeAsync(userId);

        if (result.Success)
            return Content(result.QrCodeContent, "application/json");

        return Json(new { success = false, message = result.ErrorMessage });
    }

    [HttpGet]
    public async Task<IActionResult> CheckStatus()
    {
        var userId = await _user.GetRootUserIdAsync();
        if (string.IsNullOrEmpty(userId))
            return Json(new { exists = false, isReady = false });

        var status = await _whatsAppService.GetSessionStatusAsync(userId);

        return Json(new
        {
            exists = status.SessionExists,
            isReady = status.IsConnected
        });
    }
}

