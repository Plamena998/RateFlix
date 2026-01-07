namespace RateFlix.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public List<UserReviewViewModel> Reviews { get; set; } = new();
        public List<FavoriteContentViewModel> Favorites { get; set; } = new();
        public ProfileStatisticsViewModel Statistics { get; set; } = new();

    }
}
