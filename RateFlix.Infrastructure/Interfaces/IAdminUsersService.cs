using RateFlix.Core.ViewModels.Admin;

namespace RateFlix.Services.Interfaces
{
    public interface IAdminUsersService
    {
        Task<AdminUsersIndexViewModel> GetUsersAsync(string search = "", string status = "", int page = 1, int pageSize = 20);
        Task<AdminUserDetailsViewModel?> GetUserDetailsAsync(string id);
        Task<bool> BanUserAsync(string id);
        Task<bool> UnbanUserAsync(string id);
        Task<bool> DeleteUserAsync(string id, string currentUserId);
        Task<List<AdminUserListViewModel>> GetAdminUsersAsync();
        Task<bool> MakeAdminAsync(string id);
        Task<bool> RemoveAdminAsync(string id, string currentUserId);
    }
}
