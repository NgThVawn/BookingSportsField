using BookingSportsField.Models;
using BookingSportsField.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Repository
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _db;

        public RecommendationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<FacilityRecommendationVM>> RecommendAsync(string userId)
        {
            var userBookings = await _db.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();
            var favoriteSlot = userBookings
                .GroupBy(b => b.StartTime)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .First();

            var fields = await _db.Fields
                .Include(f => f.Facility)
                .ToListAsync();

            var fieldScores = new List<(Field field, decimal score)>();

            foreach (var field in fields)
            {
                decimal score = 0;

                score += userBookings.Count(b => b.FieldId == field.Id) * 10;

                score += userBookings.Any(b =>
                    b.FieldId == field.Id &&
                    b.StartTime == favoriteSlot) ? 5 : 0;

                if (field.PricePerHour <= 500000)
                    score += 3;

                fieldScores.Add((field, score));
            }

            return fieldScores
                .GroupBy(x => x.field.FacilityId)
                .Select(g =>
                {
                    var best = g.OrderByDescending(x => x.score).First();
                    return new FacilityRecommendationVM
                    {
                        FacilityId = best.field.FacilityId,
                        FacilityName = best.field.Facility.Name,
                        Score = g.Max(x => x.score),
                        BestFieldId = best.field.Id,
                        BestFieldName = best.field.Name,
                        RecommendedStart = favoriteSlot,
                        RecommendedEnd = favoriteSlot.Add(TimeSpan.FromHours(1))
                    };
                })
                .OrderByDescending(f => f.Score)
                .Take(3)
                .ToList();
        }

    }

}
