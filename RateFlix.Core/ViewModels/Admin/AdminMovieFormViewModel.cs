using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RateFlix.Core.ViewModels.Admin
{
    public class AdminMovieFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1900, 2100)]
        public int ReleaseYear { get; set; }

        [Required]
        [Range(1, 500)]
        public int Duration { get; set; } // in minutes

        [Required]
        [Range(0, 10)]
        public double IMDBScore { get; set; }

        [Range(0, 100)]
        public double MetaScore { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        [Url]
        public string? TrailerUrl { get; set; }

        [Required]
        public int DirectorId { get; set; }

        public List<int> SelectedGenreIds { get; set; } = new();
        public List<int> SelectedActorIds { get; set; } = new();

        // For dropdowns
        public List<SelectListItem> Directors { get; set; } = new();
        public List<SelectListItem> Genres { get; set; } = new();
        public List<SelectListItem> Actors { get; set; } = new();

    }
}
