# شرح جزء الذكاء الاصطناعي (AI)

هذا الملف يشرح بشكل عملي ما الذي يفعله كل جزء داخل مجلد `Services/AiServices` في مشروع إدارة المبيعات.

المجلد الرئيسي:
`Services/AiServices`

---

## 1) تدفق التنفيذ من البداية للنهاية

عند إرسال رسالة من واجهة الشات:

1. `AiController.Stream` يستقبل الرسالة.
2. `AiContextAssembler` يبني الـ History (System Prompt + رسائل سابقة).
3. `AiOrchestratorService` يرسل الطلب للموديل عبر OpenRouter.
4. الرد يرجع Streaming ويخرج حالات مثل:
   - `[STATUS] analyzing`
   - `[STATUS] generating`
   - `[STATUS] finalizing`
5. إذا الموديل احتاج بيانات حقيقية، ينفذ Tool من خلال `AiToolExecutor`.
6. نتيجة كل Tool ترجع بشكل موحد (Envelope فيه success/data/error/meta).
7. الرسائل تُحفظ في قاعدة البيانات عبر `AiConversationService`.
8. `AiTelemetryService` يسجل مؤشرات الأداء والأخطاء.

---

## 2) شرح المجلدات والملفات

## `Contracts`
هذا المجلد يحتوي على Interfaces (العقود) لكل خدمة.

أهم الملفات:
- `IAiOrchestratorService.cs`: تشغيل الشات (رد كامل + Streaming).
- `IAiToolExecutor.cs`: تنفيذ أدوات البيانات.
- `IAiConversationService.cs`: إدارة المحادثات والرسائل.
- `IAiPromptBuilder.cs`: بناء System Prompt.
- `IAiContextAssembler.cs`: تجميع سياق المحادثة قبل الإرسال للموديل.
- `IAiTelemetryService.cs`: تسجيل القياسات الداخلية.

الفائدة:
- تسهيل الاختبار.
- فصل التنفيذ عن الواجهة.

---

## `Core`
يحتوي المنطق الأساسي لتشغيل الموديل.

الملف الأساسي:
- `AiOrchestratorService.cs`

وظيفته:
- أخذ `history` و`userMessage`.
- اختيار الموديل من `OpenRouter:Model`.
- تشغيل Streaming للرد.
- إرسال حالات تقدم (`analyzing/generating/finalizing`).
- تسجيل Telemetry (نجاح/فشل/مدة/حجم النص).

---

## `Context`
تجهيز الرسائل التي ستذهب للموديل.

الملف:
- `AiContextAssembler.cs`

وظيفته:
- إضافة System Prompt أولًا.
- جلب الرسائل السابقة للمحادثة.
- الالتزام بعدد أقصى من الرسائل حسب:
  - `AI:Context:MaxHistoryMessages`

---

## `Conversations`
إدارة المحادثات والرسائل في قاعدة البيانات.

الملف:
- `AiConversationService.cs`

وظائفه:
- إنشاء محادثة جديدة.
- جلب محادثة محددة.
- جلب كل محادثات المستخدم.
- إضافة رسالة (user/assistant).
- حذف محادثة.
- مسح كل محادثات المستخدم.
- جلب History مختصر للودجت.

ملاحظة مهمة:
- كل العمليات مربوطة بـ `TenantId` و`UserId` لضمان عزل بيانات كل عميل.

---

## `Prompting`
توليد System Prompt الثابت للمساعد.

الملف:
- `AiPromptBuilder.cs`

وظيفته:
- بناء تعليمات واضحة للموديل (الرد بالعربية، الاعتماد على بيانات حقيقية، عدم التخمين).
- تمرير عملة المتجر الحالية حسب بيانات الـ Tenant.

---

## `Tools`
هذا أهم جزء للربط مع بيانات النظام الحقيقية.

الملفات:
- `AiToolDefinitions.cs`
  - تعريف الأدوات التي يراها الموديل.
  - لكل Tool: الاسم + الوصف + البراميترز + Handle.

- `AiToolExecutor.cs`
  - التنفيذ الفعلي للاستعلامات على قاعدة البيانات.
  - التحقق من المدخلات (Validation).
  - تطبيق Timeout/Retry حسب الإعدادات.
  - إعادة النتيجة في Envelope موحد.

إعدادات الأدوات:
- `AI:Tools:TimeoutMs`
- `AI:Tools:Retries`

أمثلة أدوات:
- `get_total_sales`
- `get_top_products`
- `get_monthly_sales`
- `get_profit`
- `get_low_stock_products`
- `get_top_customers`
- `get_store_account_summary`
- `get_pending_debts`
- `get_general_statistics`
- `get_sales_report`
- `get_inventory_report`
- `get_customer_report`
- `get_financial_report`
- `get_general_debt_report`
- `get_category_performance_report`
- `get_installments_summary`
- `get_payment_methods_summary`

---

## `Telemetry`
تجميع قياسات داخلية لأداء الـ AI.

الملف:
- `AiTelemetryService.cs`

يسجل:
- عدد النجاحات والإخفاقات في Streaming.
- مدد التنفيذ.
- عدد مرات تنفيذ كل Tool.
- أنواع الأخطاء.
- Snapshot كامل للحالة الحالية.

الإعداد:
- `AI:Telemetry:Enabled`

---

## 3) دور `AiController`

الملف:
`Controllers/AiController.cs`

أهم الـ Endpoints:
- `Index()` صفحة الشات.
- `Telemetry()` صفحة متابعة القياسات.
- `TelemetrySnapshot()` API للقياسات.
- `Stream()` نقطة البث المباشر للرد.
- `GetConversations / GetConversation / GetHistory / DeleteConversation / Clear` لإدارة المحادثات.

---

## 4) الإعدادات المهمة في `appsettings.json`

- `OpenRouter:ApiKey`: مفتاح OpenRouter (يمكن أيضًا من متغير البيئة `OPENROUTER_API_KEY`).
- `OpenRouter:Model`: اسم الموديل المستخدم.
- `AI:Context:MaxHistoryMessages`: أقصى عدد رسائل History.
- `AI:Tools:TimeoutMs`: حد زمن تنفيذ الأداة.
- `AI:Tools:Retries`: عدد محاولات إعادة التنفيذ.
- `AI:Telemetry:Enabled`: تشغيل/إيقاف القياسات.

---

## 5) طريقة إضافة Tool جديد

1. أضف Params class في `AiToolDefinitions.cs`.
2. أضف Tool class جديد (Name/Description/Handle).
3. أضف case جديد في `ExecuteCoreAsync` داخل `AiToolExecutor.cs`.
4. نفذ منطق الاستعلام الفعلي.
5. أرجع النتيجة بصيغة Envelope.
6. سجّل الـ Tool في constructor داخل `AiOrchestratorService.cs`.
7. اختبره من واجهة AI.

---

## 6) نقاط قوة حالية

- تصميم طبقات واضح وسهل التطوير.
- دعم Multi-tenant بعزل بيانات مضبوط.
- Streaming وتجربة مستخدم جيدة.
- Telemetry مفيد جدًا في التشخيص.
- أدوات تقارير كثيرة تغطي أغلب احتياجات الإدارة.

لو احتجت نسخة أقصر (Quick Reference) أو رسم تدفق مختصر، أقدر أضيفها هنا مباشرة.
