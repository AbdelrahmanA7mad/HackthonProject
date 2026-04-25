using ManageMentSystem.Models;

namespace ManageMentSystem.ViewModels
{
    public class PaginatedInstallmentsViewModel
    {
        public List<Installment> Installments { get; set; } = new List<Installment>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartPage => Math.Max(1, CurrentPage - 2);
        public int EndPage => Math.Min(TotalPages, CurrentPage + 2);
    }
}
