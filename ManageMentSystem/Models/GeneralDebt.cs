using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.Models
{
    public class GeneralDebt
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "عنوان الدين")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "الطرف")]
        public string? PartyName { get; set; }

        [Required]
        [Display(Name = "نوع الدين")] // دين ليا (يستحق لي) أو دين عليا (ألتزم بسداده)
        public GeneralDebtType DebtType { get; set; }

        [Required]
        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }

        [Display(Name = "المدفوع/المحصّل")]
        public decimal PaidAmount { get; set; } = 0;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ الاستحقاق")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        public ICollection<StoreAccount> StoreAccounts { get; set; } = new List<StoreAccount>();

        // Multi-tenancy
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }


    }

    public enum GeneralDebtType
    {
        [Display(Name = "دين ليا")] // مستحق لي (Receivable)
        OwedToMe = 1,
        [Display(Name = "دين عليا")] // التزام علي (Payable)
        OnMe = 2
    }
}


