using BookingSportsField.Models;

namespace BookingSportsField.Repository
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<List<Booking>> GetAllBookings();
        Task<bool> IsAvailable(int fieldId, DateTime date, TimeSpan startTime, TimeSpan endTime);
        Task<List<Booking>> GetBookingsByUserIdAsync(string userId);
        Task<List<Booking>> GetBookingsOutdated();
        Task<List<Booking>> GetBookingsByOwnerIdAsync(string ownerId);
        Task<List<Booking>> GetBookingsByFieldIdAsync(int fieldId);
        Task<Booking> GetBookingsByUserAndFacility(int facilityId, string userId);
        Task<int> GetPendingBookingCountByOwnerAsync(string ownerId);
        Task<bool> IsSlotTaken(int fieldId, DateTime date, TimeSpan start, TimeSpan end);

    }
}
