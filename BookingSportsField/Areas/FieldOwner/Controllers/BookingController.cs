using BookingSportsField.Hubs;
using BookingSportsField.Models;
using BookingSportsField.Repository;
using BookingSportsField.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BookingSportsField.Areas.FieldOwner.Controllers
{
    [Area("FieldOwner")]
    [Authorize(Roles = "FieldOwner")]
    public class BookingController : Controller
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFieldRepository _fieldRepository;
        private readonly IFacilityRepository _facilityRepository;
        private readonly EmailService _emailService;
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly INotificationRepository _notificationRepository;
        public BookingController(IBookingRepository bookingRepository, UserManager<ApplicationUser> userManager, EmailService emailService, IFieldRepository fieldRepository, IFacilityRepository facilityRepository, IHubContext<BookingHub> hubContext, INotificationRepository notificationRepository)
        {
            _bookingRepository = bookingRepository;
            _userManager = userManager;
            _emailService = emailService;
            _fieldRepository = fieldRepository;
            _facilityRepository = facilityRepository;
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
        }
        private async Task<Notification> CreateNotificationAsync(string userId, string messageText, string targetUrl)
        {
            var storedMessage = string.IsNullOrWhiteSpace(targetUrl)
                ? messageText
                : $"{messageText} [URL]:{targetUrl}";
            var notif = new Notification
            {
                UserId = userId,
                Message = storedMessage,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            await _notificationRepository.AddAsync(notif);
            return notif;
        }
        private async Task UpdateBookingStatusesAsync()
        {
            var outdatedBookings = await _bookingRepository.GetBookingsOutdated();

            foreach (var booking in outdatedBookings)
            {
                booking.Status = BookingStatus.Cancelled;
            }

            await _bookingRepository.SaveChangesAsync();
        }
        public async Task<IActionResult> Index()
        {
            await UpdateBookingStatusesAsync();
            var user = await _userManager.GetUserAsync(User);
            var bookings = await _bookingRepository.GetBookingsByOwnerIdAsync(user.Id);
            return View(bookings);
        }
        [HttpPost]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound("Lịch đặt không tồn tại.");
            }
            booking.Status = BookingStatus.Cancelled;
            await _bookingRepository.UpdateAsync(booking);
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            var emailSubject = "Xác nhận hủy đặt sân";

            // Định dạng giờ bắt đầu và giờ kết thúc
            string startTimeFormatted = booking.StartTime.ToString(@"hh\:mm");
            string endTimeFormatted = booking.EndTime.ToString(@"hh\:mm");

            // Tạo nội dung email xác nhận
            var emailBody = $"Chúng tôi xin thông báo rằng yêu cầu đặt sân của bạn đã bị từ chối. " +
                            $"Thông tin đặt sân:<br/>" +
                            $"Cơ sở: {booking.Field.Facility.Name}<br/>" +
                            $"{booking.Field.Name}<br/>" +
                            $"Giờ: {startTimeFormatted} - {endTimeFormatted}<br/>" +
                            $"Ngày: {booking.BookingDate.ToString("dd/MM/yyyy")}<br/>" +
                            $"Địa chỉ: {booking.Field.Facility.Address}<br/>" +
                            "Trạng thái: ĐÃ HỦY<br/>" +
                            "Xin cảm ơn quý khách đã sử dụng dịch vụ, hẹ gặp lại quý khách!";
            var user = await _userManager.FindByIdAsync(booking.UserId);
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            await _hubContext.Clients
                .User(booking.UserId)
                .SendAsync("ReceiveNotification",
        $"❌ Lịch đặt của bạn tại {booking.Field.Facility.Name} đã bị hủy.");

            // Gửi email xác nhận cho người dùng
            _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            var urlCancel = Url.Action("BookingHistory", "Home", new { area = "", highlight = booking.Id }, protocol: null, host: null);
            await CreateNotificationAsync(booking.UserId, $"❌ Lịch đặt của bạn tại {booking.Field.Facility.Name} đã bị hủy.", urlCancel);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound("Lịch đặt không tồn tại.");
            }
            booking.Status = BookingStatus.Confirmed;
            await _bookingRepository.UpdateAsync(booking);
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            var emailSubject = "Xác nhận đặt sân thành công";

            // Định dạng giờ bắt đầu và giờ kết thúc
            string startTimeFormatted = booking.StartTime.ToString(@"hh\:mm");
            string endTimeFormatted = booking.EndTime.ToString(@"hh\:mm");

            // Tạo nội dung email xác nhận
            var emailBody = $"Chúng tôi xin thông báo rằng yêu cầu đặt sân của bạn đã được chấp nhận. " +
                            $"Thông tin đặt sân:<br/>" +
                            $"Cơ sở: {booking.Field.Facility.Name}<br/>" +
                            $"{booking.Field.Name}<br/>" +
                            $"Giờ: {startTimeFormatted} - {endTimeFormatted}<br/>" +
                            $"Ngày: {booking.BookingDate.ToString("dd/MM/yyyy")}<br/>" +
                            $"Địa chỉ: {booking.Field.Facility.Address}<br/>" +
                            "Xin vui lòng đến nhận sân đúng giờ, chân thành cảm ơn quy khách hàng!";
            var user = await _userManager.FindByIdAsync(booking.UserId);
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            await _hubContext.Clients
                .User(booking.UserId)
                .SendAsync("ReceiveNotification",
                    $"✅ Lịch đặt của bạn đã được xác nhận tại {booking.Field.Facility.Name}.");

            // Gửi email xác nhận cho người dùng
            _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            var urlConfirm = Url.Action("BookingHistory", "Home", new { area = "", highlight = booking.Id }, protocol: null, host: null);
            await CreateNotificationAsync(booking.UserId, $"✅ Lịch đặt của bạn đã được xác nhận tại {booking.Field.Facility.Name}.", urlConfirm);
            return RedirectToAction("Index");
        }
        
        // AJAX endpoints for in-place updates without page reload
        [HttpPost]
        public async Task<IActionResult> CancelBookingAjax(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return Json(new { success = false, message = "Lịch đặt không tồn tại." });
            }
            booking.Status = BookingStatus.Cancelled;
            await _bookingRepository.UpdateAsync(booking);
            // Notify user and send email like non-AJAX action
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            var emailSubject = "Xác nhận hủy đặt sân";
            string startTimeFormatted = booking.StartTime.ToString(@"hh\:mm");
            string endTimeFormatted = booking.EndTime.ToString(@"hh\:mm");
            var emailBody = $"Chúng tôi xin thông báo rằng yêu cầu đặt sân của bạn đã bị từ chối. " +
                            $"Thông tin đặt sân:<br/>" +
                            $"Cơ sở: {booking.Field.Facility.Name}<br/>" +
                            $"{booking.Field.Name}<br/>" +
                            $"Giờ: {startTimeFormatted} - {endTimeFormatted}<br/>" +
                            $"Ngày: {booking.BookingDate.ToString("dd/MM/yyyy")}<br/>" +
                            $"Địa chỉ: {booking.Field.Facility.Address}<br/>" +
                            "Trạng thái: ĐÃ HỦY<br/>" +
                            "Xin cảm ơn quý khách đã sử dụng dịch vụ, hẹ gặp lại quý khách!";
            var user = await _userManager.FindByIdAsync(booking.UserId);
            await _hubContext.Clients
                .User(booking.UserId)
                .SendAsync("ReceiveNotification",
                    $"❌ Lịch đặt của bạn tại {booking.Field.Facility.Name} đã bị hủy.");
            await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            var urlAjaxCancel = Url.Action("BookingHistory", "Home", new { area = "", highlight = booking.Id }, protocol: null, host: null);
            await CreateNotificationAsync(booking.UserId, $"❌ Lịch đặt của bạn tại {booking.Field.Facility.Name} đã bị hủy.", urlAjaxCancel);
            return Json(new { success = true, bookingId = booking.Id, status = (int)booking.Status });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBookingAjax(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return Json(new { success = false, message = "Lịch đặt không tồn tại." });
            }
            booking.Status = BookingStatus.Confirmed;
            await _bookingRepository.UpdateAsync(booking);
            // Notify user and send email like non-AJAX action
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            var emailSubject = "Xác nhận đặt sân thành công";
            string startTimeFormatted = booking.StartTime.ToString(@"hh\:mm");
            string endTimeFormatted = booking.EndTime.ToString(@"hh\:mm");
            var emailBody = $"Chúng tôi xin thông báo rằng yêu cầu đặt sân của bạn đã được chấp nhận. " +
                            $"Thông tin đặt sân:<br/>" +
                            $"Cơ sở: {booking.Field.Facility.Name}<br/>" +
                            $"{booking.Field.Name}<br/>" +
                            $"Giờ: {startTimeFormatted} - {endTimeFormatted}<br/>" +
                            $"Ngày: {booking.BookingDate.ToString("dd/MM/yyyy")}<br/>" +
                            $"Địa chỉ: {booking.Field.Facility.Address}<br/>" +
                            "Xin vui lòng đến nhận sân đúng giờ, chân thành cảm ơn quy khách hàng!";
            var user = await _userManager.FindByIdAsync(booking.UserId);
            await _hubContext.Clients
                .User(booking.UserId)
                .SendAsync("ReceiveNotification",
                    $"✅ Lịch đặt của bạn đã được xác nhận tại {booking.Field.Facility.Name}.");
            await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
            var urlAjaxConfirm = Url.Action("BookingHistory", "Home", new { area = "", highlight = booking.Id }, protocol: null, host: null);
            await CreateNotificationAsync(booking.UserId, $"✅ Lịch đặt của bạn đã được xác nhận tại {booking.Field.Facility.Name}.", urlAjaxConfirm);
            return Json(new { success = true, bookingId = booking.Id, status = (int)booking.Status });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsCompletedAjax(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return Json(new { success = false, message = "Lịch đặt không tồn tại." });
            }
            if (booking.Status != BookingStatus.Confirmed)
            {
                return Json(new { success = false, message = "Chỉ có thể hoàn tất các lịch đã xác nhận." });
            }
            booking.Status = BookingStatus.Completed;
            await _bookingRepository.UpdateAsync(booking);
            // Notify user like non-AJAX action
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            await _hubContext.Clients
                .User(booking.UserId)
                .SendAsync("ReceiveNotification",
                    $"🏁 Bạn đã hoàn tất buổi đá tại {booking.Field.Facility.Name}.");
            var urlCompleted = Url.Action("BookingHistory", "Home", new { area = "", highlight = booking.Id }, protocol: null, host: null);
            await CreateNotificationAsync(booking.UserId, $"🏁 Bạn đã hoàn tất buổi đá tại {booking.Field.Facility.Name}.", urlCompleted);
            return Json(new { success = true, bookingId = booking.Id, status = (int)booking.Status });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsNoShowAjax(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return Json(new { success = false, message = "Lịch đặt không tồn tại." });
            }
            if (booking.Status != BookingStatus.Confirmed)
            {
                return Json(new { success = false, message = "Chỉ có thể đánh dấu vắng mặt các lịch đã xác nhận." });
            }
            booking.Status = BookingStatus.NoShow;
            await _bookingRepository.UpdateAsync(booking);
            // Notify user like non-AJAX action
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            await _hubContext.Clients
                .User(booking.UserId)
                .SendAsync("ReceiveNotification",
                    $"⚠️ Bạn đã bị đánh dấu vắng mặt tại {booking.Field.Facility.Name}.");
            var urlNoShow = Url.Action("BookingHistory", "Home", new { area = "", highlight = booking.Id }, protocol: null, host: null);
            await CreateNotificationAsync(booking.UserId, $"⚠️ Bạn đã bị đánh dấu vắng mặt tại {booking.Field.Facility.Name}.", urlNoShow);
            return Json(new { success = true, bookingId = booking.Id, status = (int)booking.Status });
        }
        public async Task<IActionResult> Statistics(DateTime? fromDate, DateTime? toDate, int? facilityId)
        {
            var user = await _userManager.GetUserAsync(User);
            var bookings = await _bookingRepository.GetBookingsByOwnerIdAsync(user.Id);

            // Nếu không chọn ngày, mặc định lấy 30 ngày gần nhất
            fromDate ??= DateTime.Today.AddDays(-30);
            toDate ??= DateTime.Today;

            // Lọc theo ngày
            var filtered = bookings
                .Where(b => b.BookingDate.Date >= fromDate.Value.Date && b.BookingDate.Date <= toDate.Value.Date);

            // Lọc theo cơ sở nếu có chọn
            if (facilityId.HasValue)
            {
                filtered = filtered.Where(b => b.Field?.FacilityId == facilityId.Value);
            }

            var filteredList = filtered.ToList();

            var model = new BookingStatisticsViewModel
            {
                FromDate = fromDate.Value,
                ToDate = toDate.Value,
                SelectedFacilityId = facilityId,
                Facilities = (List<Facility>)await _facilityRepository.GetByOwnerIdAsync(user.Id),  // Nếu cần lấy từ DbContext
                TotalBookings = filteredList.Count,
                TotalConfirmed = filteredList.Count(b => b.Status == BookingStatus.Completed),
                TotalCancelled = filteredList.Count(b => b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.NoShow),
                TotalRevenue = filteredList
                    .Where(b => b.Status == BookingStatus.Completed)
                    .Sum(b => b.TotalPrice),
                Bookings = filteredList.OrderByDescending(b => b.BookingDate).ToList(),
                ChartData = filtered.Select(b => new BookingChartViewModel
                {
                    BookingDate = b.BookingDate,
                    Status = (int)b.Status,
                    TotalPrice = b.TotalPrice
                }).ToList()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> MarkAsCompleted(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return NotFound("Lịch đặt không tồn tại.");

            if (booking.Status != BookingStatus.Confirmed)
            {
                TempData["Error"] = "Chỉ có thể hoàn tất các lịch đã xác nhận.";
                return RedirectToAction("Index");
            }

            booking.Status = BookingStatus.Completed;
            await _bookingRepository.UpdateAsync(booking);
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            await _hubContext.Clients
                .User(booking.UserId)
                .SendAsync("ReceiveNotification",
                    $"🏁 Bạn đã hoàn tất buổi đá tại {booking.Field.Facility.Name}.");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsNoShow(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return NotFound("Lịch đặt không tồn tại.");

            if (booking.Status != BookingStatus.Confirmed)
            {
                TempData["Error"] = "Chỉ có thể đánh dấu vắng mặt các lịch đã xác nhận.";
                return RedirectToAction("Index");
            }

            booking.Status = BookingStatus.NoShow;
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            await _bookingRepository.UpdateAsync(booking);
            await _hubContext.Clients
                .User(booking.UserId)
                .SendAsync("ReceiveNotification",
                    $"⚠️ Bạn đã bị đánh dấu vắng mặt tại {booking.Field.Facility.Name}.");
            return RedirectToAction("Index");
        }

    }
}
