using BookingSportsField.Models;
using BookingSportsField.Repository;
using BookingSportsField.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StatisticController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBookingRepository _bookingRepository;
        private readonly IFacilityRepository _facilityRepository;

        public StatisticController(UserManager<ApplicationUser> userManager,
                                        IBookingRepository bookingRepo,
                                        IFacilityRepository facilityRepo)
        {
            _userManager = userManager;
            _bookingRepository = bookingRepo;
            _facilityRepository = facilityRepo;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var now = DateTime.Now;

            startDate ??= now.AddMonths(-1);
            endDate ??= now;

            var bookings = await _bookingRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();
            var facilities = await _facilityRepository.GetAsync();

            var filteredBookings = bookings
                .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate)
                .ToList();

            var statusRatios = filteredBookings
                .GroupBy(b => b.Status)
                .Select(g => new BookingStatusRatio
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                }).ToList();

            var bookingsPerDay = filteredBookings
                .GroupBy(b => b.BookingDate.Date)
                .Select(g => new DailyBookingCount
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            var mostBookedFacility = facilities
                .Select(f => new
                {
                    f.Name,
                    BookingCount = f.Fields?.Sum(field => filteredBookings.Count(b => b.FieldId == field.Id)) ?? 0
                })
                .OrderByDescending(f => f.BookingCount)
                .FirstOrDefault();

            var topUser = users
                .Select(u => new
                {
                    u.Email,
                    BookingCount = filteredBookings.Count(b => b.UserId == u.Id)
                })
                .OrderByDescending(x => x.BookingCount)
                .FirstOrDefault();

            var model = new AdminStatisticsViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalUsers = users.Count,
                StatusRatios = statusRatios,
                DailyBookings = bookingsPerDay,
                MostBookedFacilityName = mostBookedFacility?.Name,
                MostBookedFacilityCount = mostBookedFacility?.BookingCount ?? 0,
                TopUserEmail = topUser?.Email,
                TopUserBookingCount = topUser?.BookingCount ?? 0
            };

            return View(model);
        }
    }
}
