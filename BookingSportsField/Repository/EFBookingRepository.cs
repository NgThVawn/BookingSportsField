using BookingSportsField.Models;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace BookingSportsField.Repository
{
    public class EFBookingRepository : EFRepository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _context;
        public EFBookingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<bool> IsAvailable(int fieldId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            return !await _context.Bookings.AnyAsync(b =>
                b.FieldId == fieldId &&
                b.BookingDate.Date == date.Date &&
                b.Status != BookingStatus.Cancelled && // bỏ qua các booking đã huỷ
                (
                    (startTime >= b.StartTime && startTime < b.EndTime) || // bắt đầu trùng
                    (endTime > b.StartTime && endTime <= b.EndTime) || // kết thúc trùng
                    (startTime <= b.StartTime && endTime >= b.EndTime) // ôm trọn toàn bộ khung giờ cũ
                )
            );
        }
        public async Task<List<Booking>> GetBookingsByUserIdAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Field)
                .ThenInclude(f => f.Facility)
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }
        public async Task<List<Booking>> GetBookingsByOwnerIdAsync(string ownerId)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Field)
                .ThenInclude(f => f.Facility)
                .Where(b => b.Field.Facility.OwnerId == ownerId)
                .ToListAsync();
        }
        public async Task<List<Booking>> GetBookingsByFieldIdAsync(int fieldId)
        {
            return await _context.Bookings
                .Include(b => b.Field)
                .ThenInclude(f => f.Facility)
                .Where(b => b.FieldId == fieldId)
                .ToListAsync();
        }
        public async Task<List<Booking>> GetAllBookings()
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Field)
                .ThenInclude(f => f.Facility)
                .ToListAsync();
        }
        public async Task<Booking> GetBookingsByUserAndFacility(int facilityId, string userId)
        {
            return await _context.Bookings
                .Include(b => b.Field)
                .FirstOrDefaultAsync(b => b.Field.FacilityId == facilityId && b.UserId == userId);
        }
        public async Task<List<Booking>> GetBookingsOutdated()
        {
            return await _context.Bookings
                .Include(b => b.Field)
                .ThenInclude(f => f.Facility)
                .Where(b => b.BookingDate < DateTime.Now && b.Status == BookingStatus.Pending)
                .ToListAsync();
        }
        public async Task<int> GetPendingBookingCountByOwnerAsync(string ownerId)
        {
            return await _context.Bookings
                .Include(b => b.Field)
                .ThenInclude(f => f.Facility)
                .CountAsync(b => b.Field.Facility.OwnerId == ownerId && b.Status == BookingStatus.Pending);
        }
        public async Task<bool> IsSlotTaken(int fieldId, DateTime date, TimeSpan start, TimeSpan end)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.FieldId == fieldId &&
                b.BookingDate.Date == date.Date &&
                b.Status != BookingStatus.Cancelled && // bỏ qua các booking đã huỷ
                (
                    (start >= b.StartTime && start < b.EndTime) || // bắt đầu trùng
                    (end > b.StartTime && end <= b.EndTime) || // kết thúc trùng
                    (start <= b.StartTime && end >= b.EndTime) // ôm trọn toàn bộ khung giờ cũ
                )
            );
        }
    }
}
