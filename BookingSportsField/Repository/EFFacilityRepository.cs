using BookingSportsField.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Repository
{
    public class EFFacilityRepository : EFRepository<Facility>, IFacilityRepository
    {
        private readonly ApplicationDbContext _context;
        public EFFacilityRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Facility> GetByIdWithImagesAsync(int id)
        {
            return await _context.Facilities
                .Include(f => f.Images)
                .FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task<List<Facility>> GetAsync()
        {
            return await _context.Facilities
                .Include(f => f.Fields)
                .Include(f => f.Images)
                .Include(f => f.FieldOwner)
                .Include(f => f.Reviews)
                    .ThenInclude(r => r.User)
                .ToListAsync();
        }
        public async Task<string> GetMainImage(int id)
        {
            var facility = await _context.Facilities
                .Include(f => f.Images)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (facility != null && facility.Images != null && facility.Images.Count > 0)
            {
                return facility.Images.FirstOrDefault().ImageUrl;
            }
            return null;
        }
        public async Task<List<Facility>> GetByOwnerIdAsync(string ownerId)
        {
            return await _context.Facilities
                .Where(f => f.OwnerId == ownerId)
                .ToListAsync();
        }
        public async Task<List<Facility>> GetActiveAsync()
        {
            return await _context.Facilities
                .Where(f => f.ApprovalStatus == ApprovalStatus.Accepted && f.IsActive)
                .Include(f => f.Fields)
                .Include(f => f.Images)
                .Include(f => f.FieldOwner)
                .Include(f => f.Reviews)
                    .ThenInclude(r => r.User)
                .ToListAsync();
        }
    }
}
