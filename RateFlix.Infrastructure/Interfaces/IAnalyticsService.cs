using RateFlix.Core.ViewModels.Admin;

namespace RateFlix.Services
{
    public interface IAnalyticsService
    {
        Task<AdminAnalyticsViewModel> GetAdminAnalyticsAsync();
    }
}
