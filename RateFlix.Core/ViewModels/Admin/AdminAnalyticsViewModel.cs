namespace RateFlix.Core.ViewModels.Admin
{
    public class AdminAnalyticsViewModel
    {
        public List<DailyReviewStat> ReviewsThisMonth { get; set; } = new();
        public List<MonthlyReviewStat> ReviewsPerMonth { get; set; } = new();
        public double AverageReviewLength { get; set; }
        public List<RatingDistributionStat> RatingDistribution { get; set; } = new();
        public List<MostReviewedContentStat> MostReviewedContent { get; set; } = new();
        public int TotalReviews { get; set; }
        public int ReviewsWithComments { get; set; }
    }
}
