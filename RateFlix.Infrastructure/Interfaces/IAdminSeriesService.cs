using Microsoft.AspNetCore.Mvc.Rendering;
using RateFlix.Core.ViewModels.Admin;
namespace RateFlix.Services.Interfaces
{
    public interface IAdminSeriesService
    {
        Task<AdminSeriesIndexViewModel> GetSeriesAsync(string search = "", int page = 1, int pageSize = 20);
        Task<AdminSeriesFormViewModel> GetSeriesFormAsync(int? id = null);
        Task<bool> CreateSeriesAsync(AdminSeriesFormViewModel model);
        Task<bool> UpdateSeriesAsync(AdminSeriesFormViewModel model);
        Task<bool> DeleteSeriesAsync(int id);

        Task<List<SelectListItem>> GetDirectorsSelectListAsync();
        Task<List<SelectListItem>> GetGenresSelectListAsync();
        Task<List<SelectListItem>> GetActorsSelectListAsync();
    }
}
