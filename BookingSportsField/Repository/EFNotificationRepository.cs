using BookingSportsField.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Repository
{
    public class EFNotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public EFNotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification?> GetByIdForUserAsync(int id, string userId)
        {
            return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        }

        public async Task<List<Notification>> GetByUserAsync(string userId, int skip = 0, int take = 50)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
                .ToListAsync();
        }
            public async Task<int> CountUnreadAsync(string userId)
            {
                return await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .CountAsync();
            }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(Notification notification)
        {
            notification.IsRead = true;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var items = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (items.Count == 0) return;

            foreach (var n in items)
            {
                n.IsRead = true;
            }
            _context.Notifications.UpdateRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
