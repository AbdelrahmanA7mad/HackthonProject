using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManageMentSystem.Models
{
    public class Sale
    {
        public int Id { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required]
        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; }

        // إجمالي قيمة المرتجعات المرتبطة بهذه الفاتورة (يقلل من صافي قيمة الفاتورة)
        public decimal ReturnedAmount { get; set; } = 0m;

        // الرصيد المتبقي على الفاتورة (TotalAmount هنا هو الصافي بعد الخصم)
        [NotMapped]
        public decimal RemainingAmount => Math.Max(0, TotalAmount - ReturnedAmount - PaidAmount);

        // حقول الخصم
        [Range(0, 100, ErrorMessage = "نسبة الخصم يجب أن تكون بين 0 و 100")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "مبلغ الخصم يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal DiscountAmount { get; set; } = 0;

        // المبلغ بعد الخصم (يمثل TotalAmount المخزن كقيمة صافية بعد الخصم)
        public decimal AmountAfterDiscount => TotalAmount;

        public int? CustomerId { get; set; }

        public Customer? Customer { get; set; }

        // Multi-tenancy: Sale belongs to tenant
        public string? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

        // نوع الدفع للفواتير (كاش/جزئي/آجل) ويُخزن في قاعدة البيانات
        public SalePaymentType PaymentType { get; set; } = SalePaymentType.Cash;
    }

}
