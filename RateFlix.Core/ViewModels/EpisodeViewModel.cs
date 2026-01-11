using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.ViewModels
{
    public class EpisodeViewModel
    {
        public int Id { get; set; }
        public int EpisodeNumber { get; set; }
        public string Title { get; set; }
        public double IMDBScore { get; set; }
    }
}
