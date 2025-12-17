using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Data
{
    public class ScoreCalculator
    {
        // Изчислява средната IMDB оценка на сезон от епизодите
        public static async Task<double> CalculateSeasonIMDBScore(AppDbContext context, int seasonId)
        {
            var episodes = await context.Episodes
                .Where(e => e.SeasonId == seasonId)
                .ToListAsync();

            if (!episodes.Any()) return 0;

            return Math.Round(episodes.Average(e => e.IMDBScore), 1);
        }

        // Изчислява средната MetaScore на сезон от епизодите
        public static async Task<double> CalculateSeasonMetaScore(AppDbContext context, int seasonId)
        {
            var episodes = await context.Episodes
                .Where(e => e.SeasonId == seasonId)
                .ToListAsync();

            if (!episodes.Any()) return 0;

            return Math.Round(episodes.Average(e => e.MetaScore), 1);
        }

        // Изчислява средната IMDB оценка на сериал от сезоните
        public static async Task<double> CalculateSeriesIMDBScore(AppDbContext context, int seriesId)
        {
            var seasons = await context.Seasons
                .Where(s => s.SeriesId == seriesId)
                .ToListAsync();

            if (!seasons.Any()) return 0;

            return Math.Round(seasons.Average(s => s.IMDBScore), 1);
        }

        // Изчислява средната MetaScore на сериал от сезоните
        public static async Task<double> CalculateSeriesMetaScore(AppDbContext context, int seriesId)
        {
            var seasons = await context.Seasons
                .Where(s => s.SeriesId == seriesId)
                .ToListAsync();

            if (!seasons.Any()) return 0;

            return Math.Round(seasons.Average(s => s.MetaScore), 1);
        }

        // Актуализира ВСИЧКИ оценки (IMDB + Meta) за цял сериал
        public static async Task UpdateSeriesScores(AppDbContext context, int seriesId)
        {
            var seasons = await context.Seasons
                .Where(s => s.SeriesId == seriesId)
                .Include(s => s.Episodes)
                .ToListAsync();

            // Изчисляваме оценки за всеки сезон
            foreach (var season in seasons)
            {
                if (season.Episodes.Any())
                {
                    season.IMDBScore = Math.Round(season.Episodes.Average(e => e.IMDBScore), 1);
                    season.MetaScore = Math.Round(season.Episodes.Average(e => e.MetaScore), 1);
                }
            }

            await context.SaveChangesAsync();

            // Изчисляваме оценки за сериала
            var series = await context.Series.FindAsync(seriesId);
            if (series != null && seasons.Any())
            {
                series.IMDBScore = Math.Round(seasons.Average(s => s.IMDBScore), 1);
                series.MetaScore = Math.Round(seasons.Average(s => s.MetaScore), 1);
                await context.SaveChangesAsync();
            }
        }
    }
}
