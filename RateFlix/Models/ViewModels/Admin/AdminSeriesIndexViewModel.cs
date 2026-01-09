namespace RateFlix.Models.ViewModels.Admin
{
    public class AdminSeriesIndexViewModel
    {
        public List<AdminSeriesListViewModel> Series { get; set; } = new();
        public string Search { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalSeries { get; set; }
    }
}
