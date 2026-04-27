using OpenRouter.NET;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManageMentSystem.Services.AiServices
{
    /// <summary>
    /// Registry يوفر الـ ScopeFactory للـ tools عشان يقدروا يوصلوا لـ scoped services
    /// </summary>
    public static class AiToolRegistry
    {
        public static IServiceScopeFactory? ScopeFactory { get; set; }

        internal static IAiToolExecutor GetExecutor()
        {
            if (ScopeFactory == null) throw new InvalidOperationException("AiToolRegistry.ScopeFactory not set.");
            var scope = ScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IAiToolExecutor>();
        }
    }

    // ─── Params ───────────────────────────────────────────────────────────────

    public class DateRangeParams
    {
        [JsonPropertyName("from_date")] public string? FromDate { get; set; }
        [JsonPropertyName("to_date")]   public string? ToDate   { get; set; }
    }

    public class TopProductsParams
    {
        [JsonPropertyName("top_n")]     public int     TopN     { get; set; } = 5;
        [JsonPropertyName("from_date")] public string? FromDate { get; set; }
        [JsonPropertyName("to_date")]   public string? ToDate   { get; set; }
    }

    public class YearParams
    {
        [JsonPropertyName("year")] public int Year { get; set; }
    }

    public class ThresholdParams
    {
        [JsonPropertyName("threshold")] public int Threshold { get; set; } = 5;
    }

    public class TopNParams
    {
        [JsonPropertyName("top_n")] public int TopN { get; set; } = 5;
    }

    public class PeriodParams
    {
        [JsonPropertyName("period")] public string Period { get; set; } = "all";
    }

    public class EmptyParams { }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    internal static class ToolHelper
    {
        internal static string Run(string name, Dictionary<string, object> args)
        {
            using var scope = AiToolRegistry.ScopeFactory!.CreateScope();
            var exec = scope.ServiceProvider.GetRequiredService<IAiToolExecutor>();
            var result = exec.ExecuteAsync(name, args).GetAwaiter().GetResult();
            return JsonSerializer.Serialize(result);
        }
    }

    // ─── Tools ────────────────────────────────────────────────────────────────

    public class GetTotalSalesTool : Tool<DateRangeParams, string>
    {
        public override string Name        => "get_total_sales";
        public override string Description => "يجيب إجمالي المبيعات في فترة زمنية. استخدمه لأسئلة: كام المبيعات؟ إيه إجمالي الإيرادات؟";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate   != null) args["to_date"]   = p.ToDate;
            return ToolHelper.Run("get_total_sales", args);
        }
    }

    public class GetTopProductsTool : Tool<TopProductsParams, string>
    {
        public override string Name        => "get_top_products";
        public override string Description => "يجيب أكثر المنتجات مبيعاً. استخدمه لأسئلة: أكتر منتج مبيعاً؟ أفضل 5 منتجات؟";

        protected override string Handle(TopProductsParams p)
        {
            var args = new Dictionary<string, object> { ["top_n"] = p.TopN };
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate   != null) args["to_date"]   = p.ToDate;
            return ToolHelper.Run("get_top_products", args);
        }
    }

    public class GetMonthlySalesTool : Tool<YearParams, string>
    {
        public override string Name        => "get_monthly_sales";
        public override string Description => "يجيب مبيعات كل شهر في سنة معينة للتحليل الشهري";

        protected override string Handle(YearParams p) =>
            ToolHelper.Run("get_monthly_sales", new Dictionary<string, object> { ["year"] = p.Year });
    }

    public class GetProfitTool : Tool<DateRangeParams, string>
    {
        public override string Name        => "get_profit";
        public override string Description => "يجيب الربح الصافي. استخدمه لأسئلة: كام الربح؟ إيه هامش الربح؟";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate   != null) args["to_date"]   = p.ToDate;
            return ToolHelper.Run("get_profit", args);
        }
    }

    public class GetLowStockProductsTool : Tool<ThresholdParams, string>
    {
        public override string Name        => "get_low_stock_products";
        public override string Description => "يجيب المنتجات اللي مخزونها أقل من الحد المحدد. استخدمه لأسئلة: إيه المنتجات اللي خلصت؟";

        protected override string Handle(ThresholdParams p) =>
            ToolHelper.Run("get_low_stock_products", new Dictionary<string, object> { ["threshold"] = p.Threshold });
    }

    public class GetTopCustomersTool : Tool<TopNParams, string>
    {
        public override string Name        => "get_top_customers";
        public override string Description => "يجيب أفضل العملاء حسب مشترياتهم. استخدمه لأسئلة: أفضل عملائي؟ مين أكتر عميل؟";

        protected override string Handle(TopNParams p) =>
            ToolHelper.Run("get_top_customers", new Dictionary<string, object> { ["top_n"] = p.TopN });
    }

    public class GetStoreAccountSummaryTool : Tool<DateRangeParams, string>
    {
        public override string Name        => "get_store_account_summary";
        public override string Description => "يجيب ملخص الخزينة: إيرادات ومصروفات ورصيد. استخدمه لأسئلة: إيه رصيد الخزينة؟";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate   != null) args["to_date"]   = p.ToDate;
            return ToolHelper.Run("get_store_account_summary", args);
        }
    }

    public class GetPendingDebtsTool : Tool<EmptyParams, string>
    {
        public override string Name        => "get_pending_debts";
        public override string Description => "يجيب الديون والأقساط المعلقة. استخدمه لأسئلة: كام الديون عليا؟ مين لسه مديه؟";

        protected override string Handle(EmptyParams p) =>
            ToolHelper.Run("get_pending_debts", new Dictionary<string, object>());
    }

    public class GetGeneralStatisticsTool : Tool<PeriodParams, string>
    {
        public override string Name        => "get_general_statistics";
        public override string Description => "يجيب لمحة عامة عن المتجر. الفترات المتاحة: today, week, month, year, all";

        protected override string Handle(PeriodParams p) =>
            ToolHelper.Run("get_general_statistics", new Dictionary<string, object> { ["period"] = p.Period });
    }
}
