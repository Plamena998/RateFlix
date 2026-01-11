
using RateFlix.Core.Models;
using RateFlix.Infrastructure;

namespace RateFlix.Core.ViewModels
{
    public class MoviesIndexViewModel
    {
        public List<MovieCardViewModel> Movies { get; set; } = new List<MovieCardViewModel>();
        public List<Genre> Genres { get; set; } = new List<Genre>();

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalMovies { get; set; }
        public int PageSize { get; set; }

        public string? Search { get; set; }
        public int? SelectedGenreId { get; set; }
        public int? SelectedYear { get; set; }
        public string? SortBy { get; set; }
    }
}