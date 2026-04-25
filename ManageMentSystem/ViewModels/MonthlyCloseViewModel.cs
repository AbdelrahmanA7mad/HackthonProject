using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class MonthlyCloseViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "الشهر")]
        public int Month { get; set; }

        [Required]
        [Display(Name = "السنة")]
        public int Year { get; set; }

        [Display(Name = "اسم الشهر")]
        public string MonthName { get; set; } = string.Empty;

        [Display(Name = "تاريخ الإغلاق")]
        public DateTime CloseDate { get; set; }

        [Display(Name = "إجمالي المبيعات")]
        public decimal TotalSales { get; set; }

        [Display(Name = "عدد المبيعات")]
        public int SalesCount { get; set; }

        [Display(Name = "إجمالي الديون المؤقتة")]
        public decimal TotalTempMoney { get; set; }

        [Display(Name = "عدد الديون المؤقتة")]
        public int TempMoneyCount { get; set; }

        [Display(Name = "إجمالي الإيرادات")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "إجمالي التكلفة")]
        public decimal TotalCost { get; set; }

        [Display(Name = "صافي الربح")]
        public decimal NetProfit { get; set; }

        [Display(Name = "الرصيد النهائي")]
        public decimal FinalBalance { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [Display(Name = "تم الإغلاق بواسطة")]
        public string? ClosedBy { get; set; }

        public List<MonthlyCloseDetailViewModel> Details { get; set; } = new List<MonthlyCloseDetailViewModel>();
    }

    public class MonthlyCloseDetailViewModel
    {
        [Display(Name = "نوع العملية")]
        public string TransactionType { get; set; } = string.Empty;

        [Display(Name = "رقم العملية")]
        public int TransactionId { get; set; }

        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }

        [Display(Name = "التكلفة")]
        public decimal Cost { get; set; }

        [Display(Name = "الربح")]
        public decimal Profit { get; set; }

        [Display(Name = "التاريخ")]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "الوصف")]
        public string? Description { get; set; }
    }

    public class CreateMonthlyCloseViewModel
    {
        [Required]
        [Display(Name = "الشهر")]
        public int Month { get; set; }

        [Required]
        [Display(Name = "السنة")]
        public int Year { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }
    }

    public class MonthlyCloseSummaryViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal NetProfit { get; set; }
        public int TotalTransactions { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? CloseDate { get; set; }
    }
} 