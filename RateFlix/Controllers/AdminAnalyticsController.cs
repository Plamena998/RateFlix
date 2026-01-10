using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data;
using RateFlix.Models.ViewModels;
using RateFlix.Models.ViewModels.Admin;

namespace RateFlix.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminAnalyticsController : Controller
    {
        private readonly AppDbContext _context;

        public AdminAnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;

            // Reviews per day/month
            var reviewsThisMonth = await _context.Reviews
                .Where(r => r.CreatedAt.Month == now.Month && r.CreatedAt.Year == now.Year)
                .GroupBy(r => r.CreatedAt.Date)
                .Select(g => new DailyReviewStat
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Reviews per month (last 12 months)
            var reviewsPerMonth = new List<MonthlyReviewStat>();
            for (int i = 11; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var count = await _context.Reviews
                    .Where(r => r.CreatedAt.Month == date.Month && r.CreatedAt.Year == date.Year)
                    .CountAsync();

                reviewsPerMonth.Add(new MonthlyReviewStat
                {
                    Month = date.ToString("MMM yyyy"),
                    Count = count
                });
            }

            // Average review length
            var averageReviewLength = await _context.Reviews
                .Where(r => !string.IsNullOrEmpty(r.Comment))
                .AverageAsync(r => (double?)r.Comment.Length) ?? 0;

            // Rating distribution (1-10)
            var ratingDistribution = await _context.Reviews
                .GroupBy(r => r.Rating)
                .Select(g => new RatingDistributionStat
                {
                    Rating = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Rating)
                .ToListAsync();

            // Fill missing ratings with 0
            for (int i = 1; i <= 10; i++)
            {
                if (!ratingDistribution.Any(r => r.Rating == i))
                {
                    ratingDistribution.Add(new RatingDistributionStat { Rating = i, Count = 0 });
                }
            }
            ratingDistribution = ratingDistribution.OrderBy(r => r.Rating).ToList();

            // Most reviewed content
            var mostReviewedContent = await _context.Reviews
                .GroupBy(r => new { r.ContentId, r.Content.Title })
                .Select(g => new MostReviewedContentStat
                {
                    ContentTitle = g.Key.Title,
                    ReviewCount = g.Count(),
                    AverageRating = g.Average(r => r.Rating)
                })
                .OrderByDescending(x => x.ReviewCount)
                .Take(10)
                .ToListAsync();

            var model = new AdminAnalyticsViewModel
            {
                ReviewsThisMonth = reviewsThisMonth,
                ReviewsPerMonth = reviewsPerMonth,
                AverageReviewLength = Math.Round(averageReviewLength, 2),
                RatingDistribution = ratingDistribution,
                MostReviewedContent = mostReviewedContent,
                TotalReviews = await _context.Reviews.CountAsync(),
                ReviewsWithComments = await _context.Reviews.CountAsync(r => !string.IsNullOrEmpty(r.Comment))
            };

            return View(model);
        }
    }
}