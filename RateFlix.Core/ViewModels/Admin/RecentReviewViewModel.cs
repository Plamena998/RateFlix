namespace RateFlix.Core.ViewModels.Admin
{
    public class RecentReviewViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ContentTitle { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }
}
