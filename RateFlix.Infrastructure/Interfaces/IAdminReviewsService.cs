using RateFlix.Core.ViewModels.Admin;
namespace RateFlix.Services.Interfaces
{
    public interface IAdminReviewsService
    {
        Task<AdminReviewsIndexViewModel> GetReviewsAsync(string search = "", int? rating = null, int page = 1, int pageSize = 20);
        Task<bool> DeleteReviewAsync(int id);
        Task<bool> BanUserFromCommentsAsync(string userId);
    }
}
