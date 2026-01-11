namespace RateFlix.Core.ViewModels.Admin
{
    public class AdminReviewsIndexViewModel
    {
        public List<AdminReviewListViewModel> Reviews { get; set; } = new();
        public string Search { get; set; } = string.Empty;
        public int? SelectedRating { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalReviews { get; set; }
    }
}
