using System.Security.Claims;
using BookingSportsField.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSportsField.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationRepository _notificationRepository;
        public NotificationsController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        // GET: /Notifications/Go/5
        // Marks as read and redirects to embedded URL if present
        [HttpGet]
        public async Task<IActionResult> Go(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            var notif = await _notificationRepository.GetByIdForUserAsync(id, userId);
            if (notif == null)
            {
                return NotFound();
            }
            await _notificationRepository.MarkAsReadAsync(notif);

            var url = ExtractUrlFromMessage(notif.Message);
            if (string.IsNullOrWhiteSpace(url))
            {
                return RedirectToAction("Index", "Home");
            }
            return Redirect(url);
        }

        // GET: /Notifications/List (supports paging via skip/take)
        [HttpGet]
        public async Task<IActionResult> List(int skip = 0, int take = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            take = Math.Clamp(take, 1, 50);
            skip = Math.Max(0, skip);

            var unreadCount = await _notificationRepository.CountUnreadAsync(userId);
            var rawItems = await _notificationRepository.GetByUserAsync(userId, skip, take + 1); // fetch one extra to detect more
            var hasMore = rawItems.Count > take;
            var pageItems = rawItems.Take(take);

            var items = pageItems.Select(n => new
            {
                n.Id,
                n.IsRead,
                n.CreatedAt,
                message = StripUrlToken(n.Message),
                url = ExtractUrlFromMessage(n.Message)
            }).ToList();
            return Json(new { items, hasMore, unreadCount });
        }

        // POST: /Notifications/MarkAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var notif = await _notificationRepository.GetByIdForUserAsync(id, userId);
            if (notif == null)
            {
                return NotFound();
            }
            await _notificationRepository.MarkAsReadAsync(notif);
            return Json(new { success = true });
        }

        // POST: /Notifications/MarkAllRead
        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            await _notificationRepository.MarkAllAsReadAsync(userId);
            return Json(new { success = true });
        }

        private static string? ExtractUrlFromMessage(string? message)
        {
            if (string.IsNullOrEmpty(message)) return null;
            var token = "[URL]:";
            var idx = message.LastIndexOf(token, StringComparison.Ordinal);
            if (idx < 0) return null;
            return message.Substring(idx + token.Length).Trim();
        }

        private static string StripUrlToken(string? message)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;
            var token = "[URL]:";
            var idx = message.LastIndexOf(token, StringComparison.Ordinal);
            return idx < 0 ? message : message.Substring(0, idx).TrimEnd();
        }
    }
}
