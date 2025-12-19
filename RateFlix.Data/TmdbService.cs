using RateFlix.Core;
using RateFlix.Core.Tmdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RateFlix.Data
{
    public class TmdbService
    {
        private readonly HttpClient _http;
        private readonly AppOptions _options;

        public TmdbService(HttpClient http, AppOptions appOptions)
        {
            _http = http;
            _options = appOptions;
        }

        // ===== Movies =====

        public async Task<List<TmdbMovie>> GetMoviesAsync(int page)
        {
            var url = $"{_options.Url}/movie/popular?api_key={_options.ApiKey}&page={page}";
            var response = await _http.GetFromJsonAsync<TmdbApiResponse<TmdbMovie>>(url);
            return response?.Results ?? new();
        }

        public async Task<string?> GetMovieTrailerAsync(int movieId)
        {
            var url = $"{_options.Url}/movie/{movieId}/videos?api_key={_options.ApiKey}";
            var response = await _http.GetFromJsonAsync<TmdbVideoResponse>(url);
            var trailer = response?.Results
                .FirstOrDefault(v => v.Site == "YouTube" && v.Type == "Trailer");
            return trailer != null
                ? $"https://www.youtube.com/watch?v={trailer.Key}"
                : null;
        }

        public async Task<TmdbCreditsResponse?> GetMovieCreditsAsync(int movieId)
        {
            var url = $"{_options.Url}/movie/{movieId}/credits?api_key={_options.ApiKey}";
            return await _http.GetFromJsonAsync<TmdbCreditsResponse>(url);
        }

        // ===== Series =====

        public async Task<List<TmdbSeries>> GetSeriesAsync(int page)
        {
            var url = $"{_options.Url}/tv/popular?api_key={_options.ApiKey}&page={page}";
            var response = await _http.GetFromJsonAsync<TmdbApiResponse<TmdbSeries>>(url);
            return response?.Results ?? new();
        }

        public async Task<TmdbSeriesDetails?> GetSeriesDetailsAsync(int seriesId)
        {
            var url = $"{_options.Url}/tv/{seriesId}?api_key={_options.ApiKey}";
            return await _http.GetFromJsonAsync<TmdbSeriesDetails>(url);
        }

        public async Task<string?> GetSeriesTrailerAsync(int seriesId)
        {
            var url = $"{_options.Url}/tv/{seriesId}/videos?api_key={_options.ApiKey}";
            var response = await _http.GetFromJsonAsync<TmdbVideoResponse>(url);
            var trailer = response?.Results
                .FirstOrDefault(v => v.Site == "YouTube" && v.Type == "Trailer");
            return trailer != null
                ? $"https://www.youtube.com/watch?v={trailer.Key}"
                : null;
        }

        public async Task<TmdbCreditsResponse?> GetSeriesCreditsAsync(int seriesId)
        {
            var url = $"{_options.Url}/tv/{seriesId}/credits?api_key={_options.ApiKey}";
            return await _http.GetFromJsonAsync<TmdbCreditsResponse>(url);
        }

        // ===== Seasons & Episodes =====

        public async Task<TmdbSeason?> GetSeasonDetailsAsync(int seriesId, int seasonNumber)
        {
            var url = $"{_options.Url}/tv/{seriesId}/season/{seasonNumber}?api_key={_options.ApiKey}";
            return await _http.GetFromJsonAsync<TmdbSeason>(url);
        }

        // ===== Genres =====

        public async Task<List<TmdbGenre>> GetMovieGenresAsync()
        {
            var url = $"{_options.Url}/genre/movie/list?api_key={_options.ApiKey}";
            var response = await _http.GetFromJsonAsync<TmdbGenreResponse>(url);
            return response?.Genres ?? new();
        }

        public async Task<List<TmdbGenre>> GetTvGenresAsync()
        {
            var url = $"{_options.Url}/genre/tv/list?api_key={_options.ApiKey}";
            var response = await _http.GetFromJsonAsync<TmdbGenreResponse>(url);
            return response?.Genres ?? new();
        }

        // ===== Person (Actor/Director) =====

        public async Task<TmdbPerson?> GetPersonDetailsAsync(int personId)
        {
            var url = $"{_options.Url}/person/{personId}?api_key={_options.ApiKey}";
            return await _http.GetFromJsonAsync<TmdbPerson>(url);
        }
    }
}