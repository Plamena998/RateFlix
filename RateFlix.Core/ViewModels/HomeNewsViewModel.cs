using RateFlix.Core.Models;

namespace RateFlix.Core.ViewModels
{
    public class HomeNewsViewModel
    {
        public List<GenreStatViewModel> TopGenres { get; set; } = new List<GenreStatViewModel>();
        public ActorViewModel? YearActor { get; set; }
        public List<ContentViewModel> YearActorRoles { get; set; } = new List<ContentViewModel>();
        public ContentViewModel? AwardedContent { get; set; }

    }
}