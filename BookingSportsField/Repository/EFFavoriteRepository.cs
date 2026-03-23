using BookingSportsField.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Repository
{
    public class EFFavoriteRepository : EFRepository<Favorite>, IFavoriteRepository
    {
        private readonly ApplicationDbContext _context;
        public EFFavoriteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Favorite> GetFavoriteAsync(string userId, int facilityId)
        {
            return await _context.Favorites
                                 .FirstOrDefaultAsync(f => f.UserId == userId && f.FacilityId == facilityId);
        }
        public async Task<List<Facility>> GetFavoritesByUserIdAsync(string userId)
        {
            return await _context.Favorites
                                 .Where(f => f.UserId == userId)
                                 .Include(f => f.Facility)
                                 .ThenInclude(f => f.Images)
                                 .Where(f => f.Facility.IsActive)
                                 .Select(f => f.Facility)
                                 .ToListAsync();
        }
        public async Task<bool> IsFavoriteAsync(string userId, int facilityId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }
            return await _context.Favorites
                                 .AnyAsync(f => f.UserId == userId && f.FacilityId == facilityId);
        }
    }
}
