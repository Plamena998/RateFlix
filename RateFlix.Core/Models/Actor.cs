using RateFlix.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RateFlix.Core.Models;

namespace RateFlix.Core.Models
{
    public class Actor : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string? ImageUrl { get; set; }  
        public ICollection<ContentActor> ContentActors { get; set; } = new List<ContentActor>();

    }
}
