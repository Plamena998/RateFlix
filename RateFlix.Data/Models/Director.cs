using RateFlix.Data.Models;
using RateFlix.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Infrastructure
{
    public class Director : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string Name { get; set; }
        public ICollection<Movie> Movies { get; set; }
        public ICollection<Series> Series { get; set; }
        public string? ImageUrl { get; set; }  

    }
}
