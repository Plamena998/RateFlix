namespace RateFlix.Core.ViewModels.Admin
{
    public class AdminUsersIndexViewModel
    {
        public List<AdminUserListViewModel> Users { get; set; } = new();
        public string Search { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }
   
    }
}
