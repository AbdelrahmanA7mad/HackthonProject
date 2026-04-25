using System.ComponentModel.DataAnnotations;

namespace ManageMentSystem.ViewModels
{
    public class CreateCustomerViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [Display(Name = "العنوان")]
        public string Address { get; set; }
    }

    public class PaginatedCustomersViewModel
    {
        public List<Models.Customer> Customers { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; }
    }
} 