namespace RateFlix.Models.ViewModels.Admin
{
    public class AdminUserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsBanned { get; set; }
        public bool IsAdmin { get; set; }
        public int TotalReviews { get; set; }
        public int TotalFavorites { get; set; }
    }
}
