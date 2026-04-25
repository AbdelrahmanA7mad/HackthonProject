using Google.GenAI;
using Google.GenAI.Types;

namespace ManageMentSystem.Services.AiServices
{
    /// <summary>
    /// تعريفات كل الـ Functions التي يعرفها Gemini ويمكنه استدعاؤها
    /// </summary>
    public static class AiToolDefinitions
    {
        public static Tool BuildTools()
        {
            return new Tool
            {
                FunctionDeclarations = new List<FunctionDeclaration>
                {
                    // ─── المبيعات ─────────────────────────────────────────────
                    new FunctionDeclaration
                    {
                        Name = "get_total_sales",
                        Description = "يجيب إجمالي المبيعات (المبالغ) في فترة زمنية معينة. استخدمه لأسئلة مثل: كام المبيعات؟ إيه إجمالي الإيرادات؟",
                        Parameters = new Schema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, Schema>
                            {
                                ["from_date"] = new Schema { Type = "string", Description = "تاريخ البداية بصيغة yyyy-MM-dd (اختياري)" },
                                ["to_date"]   = new Schema { Type = "string", Description = "تاريخ النهاية بصيغة yyyy-MM-dd (اختياري)" }
                            }
                        }
                    },

                    new FunctionDeclaration
                    {
                        Name = "get_top_products",
                        Description = "يجيب قائمة بأكثر المنتجات مبيعاً مرتبة تنازلياً. استخدمه لأسئلة مثل: أكتر منتج مبيعاً؟ أفضل 5 منتجات؟",
                        Parameters = new Schema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, Schema>
                            {
                                ["top_n"]     = new Schema { Type = "integer", Description = "عدد المنتجات المطلوبة (افتراضي 5)" },
                                ["from_date"] = new Schema { Type = "string",  Description = "تاريخ البداية (اختياري)" },
                                ["to_date"]   = new Schema { Type = "string",  Description = "تاريخ النهاية (اختياري)" }
                            },
                            Required = new List<string> { "top_n" }
                        }
                    },

                    new FunctionDeclaration
                    {
                        Name = "get_monthly_sales",
                        Description = "يجيب مبيعات كل شهر في سنة معينة للرسم البياني والتحليل الشهري",
                        Parameters = new Schema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, Schema>
                            {
                                ["year"] = new Schema { Type = "integer", Description = "السنة المطلوبة (مثال: 2024)" }
                            },
                            Required = new List<string> { "year" }
                        }
                    },

                    // ─── الأرباح ──────────────────────────────────────────────
                    new FunctionDeclaration
                    {
                        Name = "get_profit",
                        Description = "يجيب الربح الصافي (إيرادات المبيعات - تكلفة البضاعة - المصروفات). استخدمه لأسئلة مثل: كام الربح؟ إيه هامش الربح؟",
                        Parameters = new Schema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, Schema>
                            {
                                ["from_date"] = new Schema { Type = "string", Description = "تاريخ البداية (اختياري)" },
                                ["to_date"]   = new Schema { Type = "string", Description = "تاريخ النهاية (اختياري)" }
                            }
                        }
                    },

                    // ─── المخزون ──────────────────────────────────────────────
                    new FunctionDeclaration
                    {
                        Name = "get_low_stock_products",
                        Description = "يجيب قائمة المنتجات التي مخزونها أقل من أو يساوي الحد المحدد. استخدمه لأسئلة مثل: أيه المنتجات اللي خلصت تقريباً؟ إيه اللي محتاج أشتريه؟",
                        Parameters = new Schema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, Schema>
                            {
                                ["threshold"] = new Schema { Type = "integer", Description = "الحد الأدنى للمخزون (افتراضي 5)" }
                            }
                        }
                    },

                    // ─── العملاء ──────────────────────────────────────────────
                    new FunctionDeclaration
                    {
                        Name = "get_top_customers",
                        Description = "يجيب قائمة أفضل العملاء حسب إجمالي مشترياتهم. استخدمه لأسئلة مثل: أفضل عملائي؟ مين أكتر عميل بيشتري؟",
                        Parameters = new Schema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, Schema>
                            {
                                ["top_n"] = new Schema { Type = "integer", Description = "عدد العملاء المطلوبين (افتراضي 5)" }
                            },
                            Required = new List<string> { "top_n" }
                        }
                    },

                    // ─── الخزينة ──────────────────────────────────────────────
                    new FunctionDeclaration
                    {
                        Name = "get_store_account_summary",
                        Description = "يجيب ملخص الخزينة: إجمالي الإيرادات والمصروفات والرصيد الصافي. استخدمه لأسئلة مثل: إيه رصيد الخزينة؟ كام المصروفات؟",
                        Parameters = new Schema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, Schema>
                            {
                                ["from_date"] = new Schema { Type = "string", Description = "تاريخ البداية (اختياري)" },
                                ["to_date"]   = new Schema { Type = "string", Description = "تاريخ النهاية (اختياري)" }
                            }
                        }
                    },

                    new FunctionDeclaration
                    {
                        Name = "get_pending_debts",
                        Description = "يجيب ملخص الديون والأقساط المعلقة غير المسددة. استخدمه لأسئلة مثل: كام الديون عليا؟ مين لسه مديه؟",
                        Parameters = new Schema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, Schema>()
                        }
                    },

                    // ─── الإحصائيات العامة ────────────────────────────────────
                    new FunctionDeclaration
                    {
                        Name = "get_general_statistics",
                        Description = "يجيب لمحة عامة شاملة عن المتجر: عدد المبيعات، إجمالي الإيرادات، عدد العملاء، قيمة المخزون. استخدمه للأسئلة العامة.",
                        Parameters = new Schema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, Schema>
                            {
                                ["period"] = new Schema
                                {
                                    Type = "string",
                                    Description = "الفترة: today, week, month, year, all",
                                    Enum = new List<string> { "today", "week", "month", "year", "all" }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
