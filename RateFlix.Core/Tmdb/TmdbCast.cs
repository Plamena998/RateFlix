using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbCast
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Profile_path { get; set; }
        public string Character { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
