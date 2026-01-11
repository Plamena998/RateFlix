namespace RateFlix.Core.ViewModels
{
    public class SearchItemViewModel
    {
        public int Id { get; set; }                 // Id на съдържанието
        public string Title { get; set; } = null!;  // Заглавие
        public int ReleaseYear { get; set; }        // Година на издаване
        public double IMDBScore { get; set; }       // Рейтинг IMDB
        public string? ImageUrl { get; set; }       // URL на постер/картинка
        public string Type { get; set; } = null!;   // "movie" или "series"
    }
}
