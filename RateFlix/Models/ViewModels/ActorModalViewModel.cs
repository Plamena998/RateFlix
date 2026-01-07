namespace RateFlix.Models.ViewModels
{
    public class ActorModalViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime? BirthDate { get; set; }

        public int Age
        {
            get
            {
                if (!BirthDate.HasValue) return 0;
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Value.Year;
                if (BirthDate.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        public List<ContentRoleViewModel> TopRoles { get; set; } = new();
    }

    public class ContentRoleViewModel
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public double IMDBScore { get; set; }
        public int? ReleaseYear { get; set; }
        public string? ContentType { get; set; }
    }
}