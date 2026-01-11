using RateFlix.Core.Models;

namespace RateFlix.Core.ViewModels
{
    public class ActorCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public DateTime? BirthDate { get; set; }
        public List<Content> TopContents { get; set; } = new List<Content>();

        public int Age => BirthDate.HasValue
                          ? DateTime.Today.Year - BirthDate.Value.Year -
                            (DateTime.Today < BirthDate.Value.AddYears(DateTime.Today.Year - BirthDate.Value.Year) ? 1 : 0)
                          : 0;
    }
}
