using ManageMentSystem.ViewModels;
using static ManageMentSystem.Services.StatisticsServices.StatisticsService;

namespace ManageMentSystem.Services.StatisticsServices
{
    public interface IStatisticsService
    {
        Task<StatisticsViewModel> GetStatisticsAsync();
        Task<StatisticsViewModel> GetFilteredStatisticsAsync(
            string period, 
            DateTime? fromDate, 
            DateTime? toDate, 
            int? customerId);
        Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync(int year);
        Task<List<MonthlySalesViewModel>> GetMonthlySalesAsync(int year);
        Task<ComprehensiveReportViewModel> GetComprehensiveReportAsync(DateTime? fromDate, DateTime? toDate);
        
        // تقارير جديدة شاملة
        Task<SalesReportViewModel> GetSalesReportAsync(DateTime? fromDate, DateTime? toDate, int? customerId, int? categoryId);
        Task<InventoryReportViewModel> GetInventoryReportAsync(int? categoryId, bool? lowStockOnly);
        Task<CustomerReportViewModel> GetCustomerReportAsync(DateTime? fromDate, DateTime? toDate);
        Task<FinancialReportViewModel> GetFinancialReportAsync(DateTime? fromDate, DateTime? toDate);
        Task<GeneralDebtReportViewModel> GetGeneralDebtReportAsync(DateTime? fromDate, DateTime? toDate);
        Task<StoreAccountReportViewModel> GetStoreAccountReportAsync(DateTime? fromDate, DateTime? toDate, string? transactionType);
        Task<ProfitLossReportViewModel> GetProfitLossReportAsync(DateTime? fromDate, DateTime? toDate);
        Task<CategoryPerformanceReportViewModel> GetCategoryPerformanceReportAsync(DateTime? fromDate, DateTime? toDate);
    }
} 
