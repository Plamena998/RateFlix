namespace RateFlix.Models.ViewModels.Admin
{
    public class MostReviewedContentStat
    {
        public string ContentTitle { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }
}
