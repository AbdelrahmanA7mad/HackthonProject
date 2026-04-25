using ManageMentSystem.ViewModels;

namespace ManageMentSystem.Services.HomeServices
{
    public interface IHomeService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
    }
}
