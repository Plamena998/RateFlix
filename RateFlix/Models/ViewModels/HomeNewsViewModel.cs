using RateFlix.Data.Models;
using RateFlix.Infrastructure;

namespace RateFlix.Models.ViewModels
{
    public class HomeNewsViewModel
    {
        public HomeNewsViewModel NewsSection { get; set; }
        public List<Genre> TopGenres { get; set; }    
        public Actor YearActor { get; set; }  
        public List<Content> YearActorRoles { get; set; }
        public Content AwardedContent { get; set; }
    }
}