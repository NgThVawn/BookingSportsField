using BookingSportsField.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Repository
{
    public class EFImageRepository : EFRepository<FacilityImage>, IImageRepository
    {
        private readonly ApplicationDbContext _context;
        public EFImageRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<ICollection<FacilityImage>> GetImagesByFacilityIdAsync(int facilityId)
        {
            return await _context.Images
                .Where(i => i.FacilityId == facilityId)
                .ToListAsync();
        }
    }
}
