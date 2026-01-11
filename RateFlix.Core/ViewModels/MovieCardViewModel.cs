
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.ViewModels
{
    public class MovieCardViewModel
    {
        public int Id { get; set; }               
        public string Title { get; set; } = null!;  
        public int ReleaseYear { get; set; }  
        public double IMDBScore { get; set; }    
        public string? ImageUrl { get; set; }
        public int Duration { get; set; }
    }
}
