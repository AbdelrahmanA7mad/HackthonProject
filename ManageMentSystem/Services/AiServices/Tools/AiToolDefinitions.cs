using OpenRouter.NET;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManageMentSystem.Services.AiServices
{
    public static class AiToolRegistry
    {
        public static IServiceScopeFactory? ScopeFactory { get; set; }
    }

    public class DateRangeParams
    {
        [JsonPropertyName("from_date")] public string? FromDate { get; set; }
        [JsonPropertyName("to_date")] public string? ToDate { get; set; }
    }

    public class TopProductsParams
    {
        [JsonPropertyName("top_n")] public int TopN { get; set; } = 5;
        [JsonPropertyName("from_date")] public string? FromDate { get; set; }
        [JsonPropertyName("to_date")] public string? ToDate { get; set; }
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

    public class SalesReportParams
    {
        [JsonPropertyName("from_date")] public string? FromDate { get; set; }
        [JsonPropertyName("to_date")] public string? ToDate { get; set; }
        [JsonPropertyName("customer_id")] public int? CustomerId { get; set; }
        [JsonPropertyName("category_id")] public int? CategoryId { get; set; }
    }

    public class InventoryReportParams
    {
        [JsonPropertyName("category_id")] public int? CategoryId { get; set; }
        [JsonPropertyName("low_stock_only")] public bool? LowStockOnly { get; set; }
    }

    public class InstallmentsSummaryParams
    {
        [JsonPropertyName("status")] public string? Status { get; set; }
    }

    public class EmptyParams { }

    public class CustomerSearchParams
    {
        [JsonPropertyName("customer_name")] public string Name { get; set; } = "";
    }

    public class ProductSearchParams
    {
        [JsonPropertyName("product_name")] public string Name { get; set; } = "";
    }

    public class CustomerAccountStatementParams
    {
        [JsonPropertyName("customer_name")] public string CustomerName { get; set; } = "";
        [JsonPropertyName("from_date")] public string? FromDate { get; set; }
        [JsonPropertyName("to_date")] public string? ToDate { get; set; }
        [JsonPropertyName("include_entries")] public bool? IncludeEntries { get; set; }
        [JsonPropertyName("max_entries")] public int? MaxEntries { get; set; }
    }

    public class StoreTransactionsParams
    {
        [JsonPropertyName("from_date")] public string? FromDate { get; set; }
        [JsonPropertyName("to_date")] public string? ToDate { get; set; }
        [JsonPropertyName("transaction_type")] public string? TransactionType { get; set; }
        [JsonPropertyName("category")] public string? Category { get; set; }
        [JsonPropertyName("payment_method_name")] public string? PaymentMethodName { get; set; }
        [JsonPropertyName("min_amount")] public decimal? MinAmount { get; set; }
        [JsonPropertyName("max_amount")] public decimal? MaxAmount { get; set; }
        [JsonPropertyName("limit")] public int? Limit { get; set; }
    }

    public class SalesDetailsParams
    {
        [JsonPropertyName("from_date")] public string? FromDate { get; set; }
        [JsonPropertyName("to_date")] public string? ToDate { get; set; }
        [JsonPropertyName("customer_name")] public string? CustomerName { get; set; }
        [JsonPropertyName("payment_type")] public string? PaymentType { get; set; }
        [JsonPropertyName("limit")] public int? Limit { get; set; }
    }

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

    public class GetTotalSalesTool : Tool<DateRangeParams, string>
    {
        public override string Name => "get_total_sales";
        public override string Description => "إجمالي المبيعات خلال فترة زمنية محددة.";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            return ToolHelper.Run("get_total_sales", args);
        }
    }

    public class GetTopProductsTool : Tool<TopProductsParams, string>
    {
        public override string Name => "get_top_products";
        public override string Description => "أفضل المنتجات مبيعًا حسب الكمية والإيراد.";

        protected override string Handle(TopProductsParams p)
        {
            var args = new Dictionary<string, object> { ["top_n"] = p.TopN };
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            return ToolHelper.Run("get_top_products", args);
        }
    }

    public class GetMonthlySalesTool : Tool<YearParams, string>
    {
        public override string Name => "get_monthly_sales";
        public override string Description => "الإيراد الشهري لكل شهر داخل سنة محددة.";

        protected override string Handle(YearParams p) =>
            ToolHelper.Run("get_monthly_sales", new Dictionary<string, object> { ["year"] = p.Year });
    }

    public class GetProfitTool : Tool<DateRangeParams, string>
    {
        public override string Name => "get_profit";
        public override string Description => "تحليل الربح الصافي (إيراد/تكلفة/مصروفات/هامش).";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            return ToolHelper.Run("get_profit", args);
        }
    }

    public class GetLowStockProductsTool : Tool<ThresholdParams, string>
    {
        public override string Name => "get_low_stock_products";
        public override string Description => "المنتجات منخفضة المخزون حسب حد معين.";

        protected override string Handle(ThresholdParams p) =>
            ToolHelper.Run("get_low_stock_products", new Dictionary<string, object> { ["threshold"] = p.Threshold });
    }

    public class GetTopCustomersTool : Tool<TopNParams, string>
    {
        public override string Name => "get_top_customers";
        public override string Description => "أفضل العملاء إنفاقًا.";

        protected override string Handle(TopNParams p) =>
            ToolHelper.Run("get_top_customers", new Dictionary<string, object> { ["top_n"] = p.TopN });
    }

    public class GetStoreAccountSummaryTool : Tool<DateRangeParams, string>
    {
        public override string Name => "get_store_account_summary";
        public override string Description => "ملخص الخزينة: إيراد/مصروف/صافي.";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            return ToolHelper.Run("get_store_account_summary", args);
        }
    }

    public class GetPendingDebtsTool : Tool<EmptyParams, string>
    {
        public override string Name => "get_pending_debts";
        public override string Description => "إجمالي الديون والأقساط والمديونيات المتبقية.";

        protected override string Handle(EmptyParams p) =>
            ToolHelper.Run("get_pending_debts", new Dictionary<string, object>());
    }

    public class GetGeneralStatisticsTool : Tool<PeriodParams, string>
    {
        public override string Name => "get_general_statistics";
        public override string Description => "إحصائيات عامة للمتجر حسب فترة: today/week/month/year/all.";

        protected override string Handle(PeriodParams p) =>
            ToolHelper.Run("get_general_statistics", new Dictionary<string, object> { ["period"] = p.Period });
    }

    public class GetSalesReportTool : Tool<SalesReportParams, string>
    {
        public override string Name => "get_sales_report";
        public override string Description => "تقرير مبيعات شامل مع فلاتر التاريخ والعميل والفئة.";

        protected override string Handle(SalesReportParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            if (p.CustomerId.HasValue) args["customer_id"] = p.CustomerId.Value;
            if (p.CategoryId.HasValue) args["category_id"] = p.CategoryId.Value;
            return ToolHelper.Run("get_sales_report", args);
        }
    }

    public class GetInventoryReportTool : Tool<InventoryReportParams, string>
    {
        public override string Name => "get_inventory_report";
        public override string Description => "تقرير مخزون شامل (إجماليات/نواقص/قيمة/فئات).";

        protected override string Handle(InventoryReportParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.CategoryId.HasValue) args["category_id"] = p.CategoryId.Value;
            if (p.LowStockOnly.HasValue) args["low_stock_only"] = p.LowStockOnly.Value;
            return ToolHelper.Run("get_inventory_report", args);
        }
    }

    public class GetCustomerReportTool : Tool<DateRangeParams, string>
    {
        public override string Name => "get_customer_report";
        public override string Description => "تقرير العملاء (نشط/جديد/إيراد/أفضل عملاء).";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            return ToolHelper.Run("get_customer_report", args);
        }
    }

    public class GetFinancialReportTool : Tool<DateRangeParams, string>
    {
        public override string Name => "get_financial_report";
        public override string Description => "تقرير مالي شامل (دخل/مصروف/أصول/التزامات).";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            return ToolHelper.Run("get_financial_report", args);
        }
    }

    public class GetGeneralDebtReportTool : Tool<DateRangeParams, string>
    {
        public override string Name => "get_general_debt_report";
        public override string Description => "تقرير الديون العامة (نشطة/مسددة/متبقي).";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            return ToolHelper.Run("get_general_debt_report", args);
        }
    }

    public class GetCategoryPerformanceReportTool : Tool<DateRangeParams, string>
    {
        public override string Name => "get_category_performance_report";
        public override string Description => "تقرير أداء الفئات (إيراد/ربح/هامش).";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            return ToolHelper.Run("get_category_performance_report", args);
        }
    }

    public class GetInstallmentsSummaryTool : Tool<InstallmentsSummaryParams, string>
    {
        public override string Name => "get_installments_summary";
        public override string Description => "ملخص التقسيط (عدد/إجمالي/مسدد/متبقي/حسب الحالة).";

        protected override string Handle(InstallmentsSummaryParams p)
        {
            var args = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(p.Status)) args["status"] = p.Status!;
            return ToolHelper.Run("get_installments_summary", args);
        }
    }

    public class GetPaymentMethodsSummaryTool : Tool<DateRangeParams, string>
    {
        public override string Name => "get_payment_methods_summary";
        public override string Description => "ملخص طرق الدفع (دخل/مصروف/صافي/عدد عمليات).";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            return ToolHelper.Run("get_payment_methods_summary", args);
        }
    }

    public class GetCustomerInfoTool : Tool<CustomerSearchParams, string>
    {
        public override string Name => "get_customer_info";
        public override string Description => "تفاصيل عميل محدد بالاسم: إجمالي مشترياته، ديونه، وآخر 5 معاملات له.";

        protected override string Handle(CustomerSearchParams p) =>
            ToolHelper.Run("get_customer_info", new Dictionary<string, object> { ["customer_name"] = p.Name });
    }

    public class SearchProductTool : Tool<ProductSearchParams, string>
    {
        public override string Name => "search_product";
        public override string Description => "البحث عن منتج بالاسم: سعر البيع، الكمية في المخزون، الفئة، والباركود.";

        protected override string Handle(ProductSearchParams p) =>
            ToolHelper.Run("search_product", new Dictionary<string, object> { ["product_name"] = p.Name });
    }

    public class GetExpenseDetailsTool : Tool<DateRangeParams, string>
    {
        public override string Name => "get_expense_details";
        public override string Description => "قائمة تفصيلية بالمصروفات المسجلة: اسم العملية، المبلغ، الفئة، وطريقة الدفع.";

        protected override string Handle(DateRangeParams p)
        {
            var args = new Dictionary<string, object>();
            if (p.FromDate != null) args["from_date"] = p.FromDate;
            if (p.ToDate != null) args["to_date"] = p.ToDate;
            return ToolHelper.Run("get_expense_details", args);
        }
    }

    public class GetCustomerAccountStatementTool : Tool<CustomerAccountStatementParams, string>
    {
        public override string Name => "get_customer_account_statement";
        public override string Description => "كشف حساب عميل تفصيلي (فواتير/مدفوعات/متبقي) مع فلترة تاريخية.";

        protected override string Handle(CustomerAccountStatementParams p)
        {
            var args = new Dictionary<string, object> { ["customer_name"] = p.CustomerName };
            if (!string.IsNullOrWhiteSpace(p.FromDate)) args["from_date"] = p.FromDate!;
            if (!string.IsNullOrWhiteSpace(p.ToDate)) args["to_date"] = p.ToDate!;
            if (p.IncludeEntries.HasValue) args["include_entries"] = p.IncludeEntries.Value;
            if (p.MaxEntries.HasValue) args["max_entries"] = p.MaxEntries.Value;
            return ToolHelper.Run("get_customer_account_statement", args);
        }
    }

    public class GetStoreTransactionsTool : Tool<StoreTransactionsParams, string>
    {
        public override string Name => "get_store_transactions";
        public override string Description => "قيود الخزنة التفصيلية مع فلاتر التاريخ والنوع والفئة وطريقة الدفع والمبلغ.";

        protected override string Handle(StoreTransactionsParams p)
        {
            var args = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(p.FromDate)) args["from_date"] = p.FromDate!;
            if (!string.IsNullOrWhiteSpace(p.ToDate)) args["to_date"] = p.ToDate!;
            if (!string.IsNullOrWhiteSpace(p.TransactionType)) args["transaction_type"] = p.TransactionType!;
            if (!string.IsNullOrWhiteSpace(p.Category)) args["category"] = p.Category!;
            if (!string.IsNullOrWhiteSpace(p.PaymentMethodName)) args["payment_method_name"] = p.PaymentMethodName!;
            if (p.MinAmount.HasValue) args["min_amount"] = p.MinAmount.Value;
            if (p.MaxAmount.HasValue) args["max_amount"] = p.MaxAmount.Value;
            if (p.Limit.HasValue) args["limit"] = p.Limit.Value;
            return ToolHelper.Run("get_store_transactions", args);
        }
    }

    public class GetSalesDetailsTool : Tool<SalesDetailsParams, string>
    {
        public override string Name => "get_sales_details";
        public override string Description => "فواتير المبيعات التفصيلية مع المتحصل والمتبقي والمرتجع.";

        protected override string Handle(SalesDetailsParams p)
        {
            var args = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(p.FromDate)) args["from_date"] = p.FromDate!;
            if (!string.IsNullOrWhiteSpace(p.ToDate)) args["to_date"] = p.ToDate!;
            if (!string.IsNullOrWhiteSpace(p.CustomerName)) args["customer_name"] = p.CustomerName!;
            if (!string.IsNullOrWhiteSpace(p.PaymentType)) args["payment_type"] = p.PaymentType!;
            if (p.Limit.HasValue) args["limit"] = p.Limit.Value;
            return ToolHelper.Run("get_sales_details", args);
        }
    }
}
