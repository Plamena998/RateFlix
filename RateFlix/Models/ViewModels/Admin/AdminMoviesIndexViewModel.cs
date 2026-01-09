namespace RateFlix.Models.ViewModels.Admin
{
    public class AdminMoviesIndexViewModel
    {
        public List<AdminMovieListViewModel> Movies { get; set; } = new();
        public string Search { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalMovies { get; set; }
    }
}
