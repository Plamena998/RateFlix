using RateFlix.Core.Models;
using RateFlix.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Models
{
    public class Review : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int ContentId { get; set; }
        public Content Content { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;


    }
}
