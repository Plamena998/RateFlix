namespace RateFlix.Models.ViewModels
{
    public class ContentViewModel
    {
        public int Id { get; set; }              
        public string Title { get; set; }         
        public string ImageUrl { get; set; }      
        public int ReleaseYear { get; set; }      
        public double IMDBScore { get; set; }     
        public string ContentType { get; set; } 
    }
}
