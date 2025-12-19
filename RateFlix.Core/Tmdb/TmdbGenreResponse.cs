using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbGenreResponse
    {
        public List<TmdbGenre> Genres { get; set; } = new();
    }
}
