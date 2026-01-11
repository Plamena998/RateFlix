using RateFlix.Core.ViewModels.Admin;

namespace RateFlix.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardDataAsync();
    }
}
