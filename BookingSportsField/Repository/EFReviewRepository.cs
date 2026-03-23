using BookingSportsField.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Repository
{
    public class EFReviewRepository : EFRepository<Review>, IReviewRepository
    {
        private readonly ApplicationDbContext _context;
        public EFReviewRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<ICollection<Review>> GetReviewsByFacilityIdAsync(int facilityId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.FacilityId == facilityId)
                .ToListAsync();
        }
        public async Task<decimal> AverageRating(int facilityId)
        {
            var reviews = _context.Reviews.Where(r => r.FacilityId == facilityId).ToList();
            if (!reviews.Any())
            {
                return 0;
            }
            return (decimal)reviews.Average(r => r.Rating);
        }
    }
}
