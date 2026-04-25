using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class InstallmentSummaryViewModel
    {
        [Display(Name = "إجمالي الأقساط")]
        public int TotalInstallments { get; set; }

        [Display(Name = "الأقساط النشطة")]
        public int ActiveInstallments { get; set; }

        [Display(Name = "الأقساط المكتملة")]
        public int CompletedInstallments { get; set; }

        [Display(Name = "الأقساط المتأخرة")]
        public int OverdueInstallments { get; set; }

        [Display(Name = "إجمالي المبالغ المستحقة")]
        public decimal TotalAmountOwed { get; set; }

        [Display(Name = "إجمالي المبالغ المدفوعة")]
        public decimal TotalAmountPaid { get; set; }

        [Display(Name = "إجمالي المبالغ المتبقية")]
        public decimal TotalAmountRemaining { get; set; }

        [Display(Name = "إجمالي الفوائد")]
        public decimal TotalInterest { get; set; }

        [Display(Name = "إجمالي الدفعات المقدمة")]
        public decimal TotalDownPayments { get; set; }

        [Display(Name = "إجمالي الدفعات الشهرية")]
        public decimal TotalMonthlyPayments { get; set; }

        [Display(Name = "إجمالي الشهور الإضافية")]
        public decimal TotalExtraMonths { get; set; }

        [Display(Name = "متوسط المبلغ الشهري")]
        public decimal AverageMonthlyPayment { get; set; }

        [Display(Name = "أكبر قسط")]
        public decimal LargestInstallment { get; set; }

        [Display(Name = "أصغر قسط")]
        public decimal SmallestInstallment { get; set; }

        [Display(Name = "إجمالي العملاء")]
        public int TotalCustomers { get; set; }

        [Display(Name = "إجمالي المنتجات")]
        public int TotalProducts { get; set; }

        // إحصائيات إضافية
        [Display(Name = "نسبة الأقساط المكتملة")]
        public double CompletionRate { get; set; }

        [Display(Name = "نسبة الأقساط المتأخرة")]
        public double OverdueRate { get; set; }

        [Display(Name = "إجمالي الأشهر المدفوعة")]
        public int TotalPaidMonths { get; set; }

        [Display(Name = "إجمالي الأشهر المتبقية")]
        public int TotalRemainingMonths { get; set; }

        // حقول جديدة للملخص المفصل
        [Display(Name = "المبالغ المستحقة بدون الشهر الإضافي")]
        public decimal TotalAmountWithoutExtraMonth { get; set; }

        [Display(Name = "المبالغ المستحقة مع الشهر الإضافي")]
        public decimal TotalAmountWithExtraMonth { get; set; }

        [Display(Name = "إجمالي المبالغ الأساسية")]
        public decimal TotalBaseAmount { get; set; }

        [Display(Name = "إجمالي المبالغ مع الفائدة")]
        public decimal TotalAmountWithInterest { get; set; }

        [Display(Name = "إجمالي الدفعات الشهرية المدفوعة")]
        public decimal TotalMonthlyPaymentsPaid { get; set; }

        [Display(Name = "إجمالي الدفعات الشهرية المتبقية")]
        public decimal TotalMonthlyPaymentsRemaining { get; set; }

        [Display(Name = "متوسط الفائدة")]
        public decimal AverageInterestRate { get; set; }

        [Display(Name = "إجمالي الأشهر الإضافية المطبقة")]
        public int TotalExtraMonthsApplied { get; set; }

        [Display(Name = "نسبة المبالغ المدفوعة")]
        public double PaymentPercentage { get; set; }

        [Display(Name = "نسبة المبالغ المتبقية")]
        public double RemainingPercentage { get; set; }
    }
} 