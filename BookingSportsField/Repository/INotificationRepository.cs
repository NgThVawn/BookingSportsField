using BookingSportsField.Models;

namespace BookingSportsField.Repository
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdForUserAsync(int id, string userId);
        Task<List<Notification>> GetByUserAsync(string userId, int skip = 0, int take = 50);
        Task<int> CountUnreadAsync(string userId);
        Task AddAsync(Notification notification);
        Task MarkAsReadAsync(Notification notification);
        Task MarkAllAsReadAsync(string userId);
    }
}
