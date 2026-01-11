namespace RateFlix.Core.ViewModels
{
    public class ActorsPageViewModel
    {
        public List<ActorCardViewModel> BirthdayActors { get; set; } = new();
        public List<ActorCardViewModel> AllActors { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalActors { get; set; }
        public int PageSize { get; set; } = 30;
    }

    public class ActorsLoadPageResponse
    {
        public List<ActorCardViewModel> Actors { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}