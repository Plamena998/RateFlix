using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Data.Models
{
    public class ContentActor 
    {
        public int ContentId { get; set; }
        public Content Content { get; set; } = null!;

        public int ActorId { get; set; }
        public Actor Actor { get; set; } = null!;
    }
}
