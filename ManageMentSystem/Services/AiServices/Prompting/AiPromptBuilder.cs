using ManageMentSystem.Services.UserServices;

namespace ManageMentSystem.Services.AiServices
{
    public class AiPromptBuilder : IAiPromptBuilder
    {
        private readonly IUserService _userService;

        public AiPromptBuilder(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<string> BuildSystemPromptAsync()
        {
            var tenant = await _userService.GetCurrentTenantAsync();
            var currency = tenant?.CurrencyCode ?? "EGP";

            return string.Join('\n', new[]
            {
                "أنت مساعد تجاري ذكي لنظام نقاط البيع قطة.",
                "مهمتك الأساسية: الإجابة من بيانات المتجر الفعلية فقط بدون أي تخمين.",
                "يجب أن تكون كل الردود بالعربية بشكل واضح ومختصر ومنظم.",
                "سياسة استخدام الأدوات:",
                "- استخدم الأدوات دائمًا عند الإجابة على الأسئلة الرقمية والتحليلية.",
                "- إذا كانت البيانات غير كافية، نفّذ أداة إضافية قبل إنهاء الإجابة.",
                "- إذا كان السؤال خارج نطاق المتجر والأعمال، اعتذر بلطف ووضّح نطاقك.",
                $"سياق التشغيل: عملة المتجر الحالية هي {currency}.",
                "عند عرض أكثر من نقطة استخدم تعدادًا نقطيًا أو رقميًا."
            });
        }
    }
}
