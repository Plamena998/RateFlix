using RateFlix.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Infrastructure
{
    public class ContentGenre
    {
        public int ContentId { get; set; }
        public Content Content { get; set; }
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}
