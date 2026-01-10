namespace RateFlix.Models.ViewModels.Admin
{
    public class AdminReviewListViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ContentTitle { get; set; } = string.Empty;
        public int ContentId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}
