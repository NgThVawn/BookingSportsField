using BookingSportsField.Models;
using BookingSportsField.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookingSportsField.Areas.FieldOwner.Controllers
{
    [Area("FieldOwner")]
    [Authorize(Roles = "FieldOwner")]
    public class DashboardController : Controller
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public DashboardController(IBookingRepository bookingRepository, UserManager<ApplicationUser> userManager)
        {
            _bookingRepository = bookingRepository;
            _userManager = userManager;
        }
        public async Task<IActionResult> IndexAsync()
        {
            var currentUserId = _userManager.GetUserId(User);

            // Lấy số lượt booking chờ duyệt của các sân do user hiện tại sở hữu
            var pendingBookings = await _bookingRepository.GetPendingBookingCountByOwnerAsync(currentUserId);
            ViewBag.PendingBookingCount = pendingBookings;
            return View();
        }
    }
}
