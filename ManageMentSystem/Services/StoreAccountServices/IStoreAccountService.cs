using ManageMentSystem.Models;
using ManageMentSystem.ViewModels;

namespace ManageMentSystem.Services.StoreAccountServices
{
    public interface IStoreAccountService
    {
        Task<List<StoreAccount>> GetAllTransactionsAsync();
        Task<StoreAccount?> GetTransactionByIdAsync(int id);
        Task<StoreAccount> CreateTransactionAsync(StoreAccountViewModel model);
        Task<StoreAccount> UpdateTransactionAsync(int id, StoreAccountViewModel model);
        Task DeleteTransactionAsync(int id);
        Task<StoreAccountSummaryViewModel> GetAccountSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<StoreAccount>> GetFilteredTransactionsAsync(StoreAccountFilterViewModel filter);
        Task<decimal> GetTotalIncomeAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetTotalExpensesAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetTotalCapitalAsync();
        
        // Optimized method to get all summary data in one query
        Task<(decimal TotalIncome, decimal TotalExpenses, decimal TotalCapital, decimal CashBalance)> GetSummaryDataAsync();
        Task<List<MonthlySummary>> GetMonthlySummaryAsync(int year);
        Task<List<CategorySummary>> GetCategorySummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task AutoCreateTransactionFromSaleAsync(Sale sale);
        Task<decimal> GetCashBalanceAsync();
        Task<decimal> GetCashBalanceByPaymentMethodAsync(int? paymentMethodId);
        Task AutoCreateTransactionFromGeneralDebtAsync(GeneralDebt debt, StoreAccountViewModel model);
        Task<(decimal Receivables, decimal Payables, decimal Net)> GetGeneralDebtsSummaryAsync();
        
     
    }
} 
