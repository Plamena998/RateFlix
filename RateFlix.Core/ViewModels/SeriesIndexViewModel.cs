
using RateFlix.Core.Models;
using RateFlix.Infrastructure;

namespace RateFlix.Core.ViewModels
{
    public class SeriesIndexViewModel
    {
        public List<Series> Series { get; set; } = new();
        public List<Genre> Genres { get; set; } = new();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalSeries { get; set; }
        public int PageSize { get; set; }

        public string? Search { get; set; }
        public int? SelectedGenreId { get; set; }
        public int? SelectedYear { get; set; }
        public string? SortBy { get; set; }
    }
}