using Microsoft.AspNetCore.Mvc.Rendering;
using RateFlix.Core.ViewModels.Admin;

namespace RateFlix.Services.Interfaces
{
    public interface IAdminMovieService
    {
        Task<AdminMoviesIndexViewModel> GetMoviesAsync(string search = "", int page = 1, int pageSize = 20);
        Task<AdminMovieFormViewModel> GetMovieFormAsync(int? id = null);
        Task<bool> CreateMovieAsync(AdminMovieFormViewModel model);
        Task<bool> UpdateMovieAsync(AdminMovieFormViewModel model);
        Task<bool> DeleteMovieAsync(int id);

        Task<List<SelectListItem>> GetDirectorsSelectListAsync();
        Task<List<SelectListItem>> GetGenresSelectListAsync();
        Task<List<SelectListItem>> GetActorsSelectListAsync();
    }
}
