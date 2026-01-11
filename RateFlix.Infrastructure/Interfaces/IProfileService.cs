using RateFlix.Core.ViewModels;

namespace RateFlix.Services.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileViewModel?> GetUserProfileAsync(string userId);
        Task<ToggleFavoriteResponse> ToggleFavoriteAsync(string userId, int contentId);
        Task<bool> IsFavoriteAsync(string userId, int contentId);
    }
}
