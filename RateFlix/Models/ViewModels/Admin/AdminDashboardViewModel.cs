namespace RateFlix.Models.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ActiveUsers { get; set; }

        public int TotalMovies { get; set; }
        public int TotalSeries { get; set; }
        public int TotalActors { get; set; }

        public int TotalReviews { get; set; }
        public int ReviewsThisMonth { get; set; }

        public List<RecentReviewViewModel> RecentReviews { get; set; } = new();
        public List<RecentUserViewModel> RecentUsers { get; set; } = new();

    }
}
