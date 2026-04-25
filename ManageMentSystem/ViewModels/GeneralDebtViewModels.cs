using System.ComponentModel.DataAnnotations;
using ManageMentSystem.Models;

namespace ManageMentSystem.ViewModels
{
    public class CreateGeneralDebtViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "عنوان الدين")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "الطرف")]
        public string? PartyName { get; set; }

        [Required]
        [Display(Name = "نوع الدين")]
        public GeneralDebtType DebtType { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }

        [Display(Name = "المدفوع/المحصّل")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "تاريخ الاستحقاق")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Display(Name = "طريقة الدفع")]
        public int? PaymentMethodId { get; set; }
    }

    public class GeneralDebtListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? PartyName { get; set; }
        public GeneralDebtType DebtType { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Remaining => Amount - PaidAmount;
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Description { get; set; }
    }

    public class GeneralDebtIndexViewModel
    {
        public List<GeneralDebtListItemViewModel> Debts { get; set; } = new();
        public decimal TotalReceivables { get; set; }
        public decimal TotalPayables { get; set; }
        public decimal NetPosition => TotalReceivables - TotalPayables;
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
    }
}


