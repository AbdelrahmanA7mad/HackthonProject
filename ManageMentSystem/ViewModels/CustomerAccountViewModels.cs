using ManageMentSystem.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CustomerPaymentInputViewModel
    {
        [Required]
        public int CustomerId { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? DiscountAmount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int? PaymentMethodId { get; set; }
        public string? Notes { get; set; }
    }

    public class CustomerStatementViewModel
    {
        public int CustomerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<Sale> Sales { get; set; } = new();
        public List<CustomerPayment> Payments { get; set; } = new();

        public decimal TotalSales { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public decimal TotalSalesAmount { get; set; } // إجمالي الفواتير المبيعات
        public decimal TotalReturnsAmount { get; set; } // إجمالي الفواتير المرتجعات
        public decimal NetSalesAmount { get; set; } // صافي المبيعات (المبيعات - المرتجعات)
    }

    public class CustomerAccountEntry
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "فاتورة" أو "دفعة"
        public decimal Amount { get; set; }
        public int? SaleId { get; set; }
        public int? PaymentId { get; set; }
        public int? ReturnId { get; set; }
        public string? Notes { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? SalePaymentTypeName { get; set; }
        public decimal RunningSales { get; set; }
        public decimal RunningPaid { get; set; }
        public decimal RunningBalance { get; set; }
        public List<PaymentAllocationSummary> Allocations { get; set; } = new();
    }

    public class CustomerFullAccountViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public List<CustomerAccountEntry> Entries { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalSalesAmount { get; set; } // إجمالي الفواتير المبيعات
        public decimal TotalReturnsAmount { get; set; } // إجمالي الفواتير المرتجعات
        public decimal NetSalesAmount { get; set; } // صافي المبيعات (المبيعات - المرتجعات)

        public int Id { get; set; }

        [Required]
        [Display(Name = "اسم الشركة")]
        public string CompanyName { get; set; } = "شركة غير محددة";

        [Display(Name = "العنوان الفرعي")]
        public string? CompanySubtitle { get; set; } = "خدمات الفواتير";

        [Display(Name = "أرقام الهاتف")]
        public List<string> PhoneNumbers { get; set; } = new List<string> { "01000000000", "01100000000" };

        [Display(Name = "العنوان")]
        public string? Address { get; set; } = "القاهرة، مصر";

        [Display(Name = "رسالة التذييل")]
        public string? FooterMessage { get; set; } = "شكرًا لتعاملكم معنا";

        [Display(Name = "شعار الشركة")]
        public string? Logo { get; set; } = "/images/default-logo.png";

        [Display(Name = "الموقع الإلكتروني")]
        public string? Website { get; set; } = "https://example.com";

        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; } = "info@example.com";

        [Display(Name = "رقم المستخدم")]
        public string UserId { get; set; } = string.Empty;
        [ValidateNever]   // ✅ الحل هنا
        public ApplicationUser User { get; set; } = null!;
    }

    public class PaymentAllocationSummary
    {
        public int SaleId { get; set; }
        public decimal Amount { get; set; }
    }

    public class UnpaidSalesViewModel
    {
        public List<Sale> Sales { get; set; } = new();
        public decimal TotalUnpaidAmount { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}


