using BookingSportsField.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Repository
{
    public class EFFieldRepository : EFRepository<Field>, IFieldRepository
    {
        private readonly ApplicationDbContext _context;

        public EFFieldRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Field> GetByIdAsync(int id)
        {
            return await _context.Fields.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<List<Field>> GetFieldsByFacilityIdAsync(int facilityId)
        {
            return await _context.Fields
                .Where(f => f.FacilityId == facilityId)
                .ToListAsync();
        }

        public async Task<List<Field>> GetAvailableFields(int facilityId, DateTime date, int startHour, int endHour)
        {
            var startTime = new TimeSpan(startHour, 0, 0);
            var endTime = new TimeSpan(endHour, 0, 0);

            return await _context.Fields
                .Where(f => f.FacilityId == facilityId && f.IsActive == true)
                .Include(f => f.Bookings)
                .Where(f => !f.Bookings.Any(b =>
                    b.BookingDate.Date == date.Date &&
                    !(b.EndTime <= startTime || b.StartTime >= endTime) && // giao nhau thời gian
                    b.Status != BookingStatus.Cancelled // bỏ qua lịch đã hủy
                ))
                .ToListAsync();
        }
    }
}
