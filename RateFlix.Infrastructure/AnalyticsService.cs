using Microsoft.EntityFrameworkCore;
using RateFlix.Core.ViewModels.Admin;
using RateFlix.Data;

namespace RateFlix.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminAnalyticsViewModel> GetAdminAnalyticsAsync()
        {
            var now = DateTime.UtcNow;

            // Reviews for current year
            var reviewStats = await _context.Reviews
                .Include(r => r.Content)
                .Where(r => r.CreatedAt.Year == now.Year)
                .ToListAsync();

            // Reviews per day (this month)
            var reviewsThisMonth = reviewStats
                .Where(r => r.CreatedAt.Month == now.Month)
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new DailyReviewStat
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Reviews per month (last year)
            var reviewsPerMonthRaw = reviewStats
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToList();

            var reviewsPerMonth = Enumerable.Range(0, 12)
                .Select(i => now.AddMonths(-i))
                .Reverse()
                .Select(d => new MonthlyReviewStat
                {
                    Month = d.ToString("MMM yyyy"),
                    Count = reviewsPerMonthRaw
                        .FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Count ?? 0
                })
                .ToList();

            // Average review length
            var averageReviewLength = reviewStats
                .Where(r => !string.IsNullOrEmpty(r.Comment))
                .Select(r => r.Comment.Length)
                .DefaultIfEmpty(0)
                .Average();

            // Rating distribution (1-10)
            var ratingDistribution = reviewStats
                .GroupBy(r => r.Rating)
                .Select(g => new RatingDistributionStat { Rating = g.Key, Count = g.Count() })
                .ToList();

            for (int i = 1; i <= 10; i++)
            {
                if (!ratingDistribution.Any(r => r.Rating == i))
                    ratingDistribution.Add(new RatingDistributionStat { Rating = i, Count = 0 });
            }
            ratingDistribution = ratingDistribution.OrderBy(r => r.Rating).ToList();

            // Most reviewed content
            var mostReviewedContent = reviewStats
                .GroupBy(r => new { r.ContentId, r.Content.Title })
                .Select(g => new MostReviewedContentStat
                {
                    ContentTitle = g.Key.Title,
                    ReviewCount = g.Count(),
                    AverageRating = g.Average(r => r.Rating)
                })
                .OrderByDescending(x => x.ReviewCount)
                .Take(10)
                .ToList();

            return new AdminAnalyticsViewModel
            {
                ReviewsThisMonth = reviewsThisMonth,
                ReviewsPerMonth = reviewsPerMonth,
                AverageReviewLength = Math.Round(averageReviewLength, 2),
                RatingDistribution = ratingDistribution,
                MostReviewedContent = mostReviewedContent,
                TotalReviews = reviewStats.Count,
                ReviewsWithComments = reviewStats.Count(r => !string.IsNullOrEmpty(r.Comment))
            };
        }
    }
}
