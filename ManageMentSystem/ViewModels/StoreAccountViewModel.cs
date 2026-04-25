using ManageMentSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class StoreAccountViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العملية مطلوب")]
        [Display(Name = "اسم العملية")]
        public string TransactionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع العملية مطلوب")]
        [Display(Name = "نوع العملية")]
        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Display(Name = "المبلغ")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر")]
        public decimal Amount { get; set; }

        [Display(Name = "التاريخ")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Display(Name = "الفئة")]
        public string? Category { get; set; }

        [Display(Name = "طريقة الدفع")]
        public int? PaymentMethodId { get; set; }

        [Display(Name = "مرجع العملية")]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        // تم إزالة خانة رأس المال من الواجهة
        public decimal Capital { get; set; } = 0;

        // Ownership: track which user owns this transaction
        public string? OwnerUserId { get; set; }
        public ApplicationUser? OwnerUser { get; set; }
        // Related entity IDs
        public int? SaleId { get; set; }
    }

    public class StoreAccountSummaryViewModel
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit => TotalIncome - TotalExpenses;
        public decimal ProfitPercentage => TotalIncome > 0 ? (NetProfit / TotalIncome) * 100 : 0;
        public decimal TotalCapital { get; set; }
        public decimal CurrentBalance => TotalCapital + NetProfit;

        public List<StoreAccountViewModel> RecentTransactions { get; set; } = new List<StoreAccountViewModel>();
        public List<MonthlySummary> MonthlySummaries { get; set; } = new List<MonthlySummary>();
        public List<CategorySummary> CategorySummaries { get; set; } = new List<CategorySummary>();
    }

    public class MonthlySummary
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetProfit => Income - Expenses;
    }

    public class CategorySummary
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public TransactionType Type { get; set; }
    }

    public class StoreAccountFilterViewModel
    {
        [Display(Name = "من تاريخ")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "إلى تاريخ")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "نوع العملية")]
        public TransactionType? TransactionType { get; set; }

        [Display(Name = "الفئة")]
        public string? Category { get; set; }

        [Display(Name = "طريقة الدفع")]
        public int? PaymentMethodId { get; set; }

        [Display(Name = "الحد الأدنى للمبلغ")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "الحد الأقصى للمبلغ")]
        public decimal? MaxAmount { get; set; }
    }
} 