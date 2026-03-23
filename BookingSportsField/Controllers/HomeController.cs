using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using BookingSportsField.Hubs;
using BookingSportsField.Models;
using BookingSportsField.Repository;
using BookingSportsField.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IFacilityRepository _facilityRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IImageRepository _imageRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly EmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<BookingHub> _hubContext;
        private readonly IRecommendationService _recommendationService;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext, IFacilityRepository facilityRepository, 
            IFieldRepository fieldRepository, IBookingRepository bookingRepository, IImageRepository imageRepository, 
            IReviewRepository reviewRepository, UserManager<ApplicationUser> userManager, IRepository<Payment> paymentRepository, 
            EmailService emailService, IFavoriteRepository favoriteRepository, IHubContext<BookingHub> hubContext, INotificationRepository notificationRepository, IRecommendationService recommendationService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _facilityRepository = facilityRepository;
            _fieldRepository = fieldRepository;
            _bookingRepository = bookingRepository;
            _imageRepository = imageRepository;
            _reviewRepository = reviewRepository;
            _userManager = userManager;
            _paymentRepository = paymentRepository;
            _emailService = emailService;
            _favoriteRepository = favoriteRepository;
            _hubContext = hubContext;
            _notificationRepository = notificationRepository;
            _recommendationService = recommendationService;
        }

        private async Task CreateNotificationAsync(string userId, string messageText, string? targetUrl)
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
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            // Điều hướng nếu đã đăng nhập và có role
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (roles.Contains("FieldOwner"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "FieldOwner" });
                }
            }

            // Lấy danh sách cơ sở đang hoạt động
            var facilities = await _facilityRepository.GetActiveAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                var aiRecommendations = await _recommendationService.RecommendAsync(userId);
                ViewBag.AIRecommendations = aiRecommendations;
                var favorites = await _favoriteRepository.GetFavoritesByUserIdAsync(userId);
                var top3Favorites = favorites
                    .Take(3)
                    .Select(f => new FacilityCardViewModel
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Address = f.Address,
                        OpeningTime = f.OpeningTime,
                        ClosingTime = f.ClosingTime,
                        AverageRating = _reviewRepository.AverageRating(f.Id).Result,
                        ImageUrl = _facilityRepository.GetMainImage(f.Id).Result ?? "/images/default-facility.jpg",
                        IsFavorite = true
                    }).ToList();

                ViewBag.Favorites = top3Favorites;
            }
            else
            {
                ViewBag.Favorites = new List<FacilityCardViewModel>();
            }

            // ----------------------------
            // Cơ sở nổi bật
            // ----------------------------
            var featuredFacilities = facilities
                .OrderByDescending(f => _reviewRepository.AverageRating(f.Id).Result)
                .Take(3)
                .Select(f => new FacilityCardViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    Address = f.Address,
                    OpeningTime = f.OpeningTime,
                    ClosingTime = f.ClosingTime,
                    AverageRating = _reviewRepository.AverageRating(f.Id).Result,
                    ImageUrl = _facilityRepository.GetMainImage(f.Id).Result ?? "/images/default-facility.jpg",
                    IsFavorite = userId != null && _favoriteRepository.IsFavoriteAsync(userId, f.Id).Result
                }).ToList();

            ViewBag.FeaturedFacilities = featuredFacilities;
            // Tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                facilities = facilities
                    .Where(f => f.Name.ToLower().Contains(searchTerm) || f.Address.ToLower().Contains(searchTerm))
                    .ToList();
                ViewBag.Favorites = new List<FacilityCardViewModel>();
                ViewBag.FeaturedFacilities = new List<FacilityCardViewModel>();
            }
            var model = facilities.Select(f => new FacilityCardViewModel
            {
                Id = f.Id,
                Name = f.Name,
                Address = f.Address,
                OpeningTime = f.OpeningTime,
                ClosingTime = f.ClosingTime,
                AverageRating = _reviewRepository.AverageRating(f.Id).Result,
                ImageUrl = _facilityRepository.GetMainImage(f.Id).Result ?? "/images/default-facility.jpg",
                IsFavorite = userId != null && _favoriteRepository.IsFavoriteAsync(userId, f.Id).Result
            }).ToList();
            return View(model);
        }

        public async Task<IActionResult> AvailableFields(int facilityId, DateTime date, int startHour, int endHour, string fieldType)
        {
            if (!Enum.TryParse<FieldType>(fieldType, out var parsedType))
            {
                return BadRequest("Loại sân không hợp lệ.");
            }
            var fields = await _fieldRepository.GetAvailableFields(facilityId, date, startHour, endHour);

            var result = fields.Where(f => f.Type == parsedType).Select(f => new {
                id = f.Id,
                name = f.Name,
                type = f.Type.ToString(),
                pricePerHour = f.PricePerHour
            });
            return Json(result);
        }
        public async Task<IActionResult> Booking(int id)
        {
            var facility = await _facilityRepository.GetByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            facility.Fields = await _fieldRepository.GetFieldsByFacilityIdAsync(id);

            facility.Images = await _imageRepository.GetImagesByFacilityIdAsync(id);

            facility.Reviews = await _reviewRepository.GetReviewsByFacilityIdAsync(id);
            var otherFacilities = await _facilityRepository.GetAsync();
            otherFacilities = otherFacilities.Where(f => f.Id != id).Take(3).ToList();

            var OtherFacilities = otherFacilities.Select(f => new FacilityCardViewModel
            {
                Id = f.Id,
                Name = f.Name,
                Address = f.Address,
                OpeningTime = f.OpeningTime,
                ClosingTime = f.ClosingTime,
                AverageRating = _reviewRepository.AverageRating(f.Id).Result,
                ImageUrl = _facilityRepository.GetMainImage(f.Id).Result ?? "/images/default-facility.jpg"
            }).ToList();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, facility.Id);

            ViewBag.IsFavorite = isFavorite;
            ViewBag.OtherFacilities = OtherFacilities;
            return View(facility);
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Booking(int fieldId, DateTime bookingDate, string startTime, string endTime)
        {
            var field = await _fieldRepository.GetByIdAsync(fieldId);
            if (field == null)
            {
                return NotFound("Sân không tồn tại");
            }

            // Convert startTime and endTime to TimeSpan
            var startTimeSpan = TimeSpan.Parse(startTime);
            var endTimeSpan = TimeSpan.Parse(endTime);

            // Tính tiền cho booking
            var hours = (decimal)(endTimeSpan - startTimeSpan).TotalHours;
            var totalPrice = field.PricePerHour * hours;

            // Tạo booking mới
            var booking = new Booking
            {
                FieldId = fieldId,
                UserId = _userManager.GetUserId(User),
                BookingDate = bookingDate,
                StartTime = startTimeSpan,
                EndTime = endTimeSpan,
                TotalPrice = totalPrice,
                Status = BookingStatus.Pending
            };
            var bookingJson = JsonSerializer.Serialize(booking);
            HttpContext.Session.SetString("BookingSession", bookingJson);
            return RedirectToAction("Checkout");
        }
        public async Task<IActionResult> Checkout()
        {
            var bookingJson = HttpContext.Session.GetString("BookingSession");
            if (string.IsNullOrEmpty(bookingJson)) return RedirectToAction("Index", "Home");

            var booking = JsonSerializer.Deserialize<Booking>(bookingJson);

            booking.UserId = _userManager.GetUserId(User);
            booking.User = await _userManager.FindByIdAsync(booking.UserId);
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);

            return View(booking);
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(PaymentMethod paymentMethod)
        {
            var bookingJson = HttpContext.Session.GetString("BookingSession");
            if (string.IsNullOrEmpty(bookingJson))
                return RedirectToAction("Index", "Home");

            var booking = JsonSerializer.Deserialize<Booking>(bookingJson);
            booking.UserId = _userManager.GetUserId(User);

            // ✅ 1. Bắt đầu transaction để chống race condition
            using var tx = await _dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable
            );

            // ✅ 2. Kiểm tra slot có bị người khác đặt chưa
            bool isConflict = await _bookingRepository.IsSlotTaken(
                booking.FieldId,
                booking.BookingDate,
                booking.StartTime,
                booking.EndTime
            );

            if (isConflict)
            {
                await tx.RollbackAsync();

                TempData["Error"] = "❌ Khung giờ đã bị người khác đặt trước đó, vui lòng chọn lại!";

                var f = await _fieldRepository.GetByIdAsync(booking.FieldId);
                return RedirectToAction("Booking", new { id = f.FacilityId });
            }

            await _bookingRepository.AddAsync(booking);

            await tx.CommitAsync();

            await _hubContext.Clients.Group("Admin")
                .SendAsync("ReceiveNotification", $"📢 Có lịch đặt mới: #{booking.Id}");

            var field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            var facility = await _facilityRepository.GetByIdAsync(field.FacilityId);
            var ownerId = facility.OwnerId;

            if (!string.IsNullOrEmpty(ownerId))
            {
                await _hubContext.Clients.User(ownerId)
                    .SendAsync("ReceiveNotification", $"📢 Sân của bạn có lịch đặt mới: {facility.Name} - {field.Name}");
                // Persist notification for owner with link to owner booking page
                var ownerUrl = Url.Action("Index", "Booking", new { area = "FieldOwner" }, protocol: null, host: null);
                await CreateNotificationAsync(ownerId, $"📢 Sân của bạn có lịch đặt mới: {facility.Name} - {field.Name}", ownerUrl);
            }

            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = booking.TotalPrice,
                Method = paymentMethod,
                isPaid = false,
                PaymentDate = DateTime.Now
            };

            await _paymentRepository.AddAsync(payment);

            HttpContext.Session.Remove("BookingSession");

            return RedirectToAction("Confirmation", new { bookingId = booking.Id });
        }

        // Trang xác nhận
        public async Task<IActionResult> Confirmation(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound("Lịch đặt không tồn tại.");
            }
            booking.Field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            booking.Field.Facility = await _facilityRepository.GetByIdAsync(booking.Field.FacilityId);
            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int facilityId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Home") });
            }
            var existingFavorite = await _favoriteRepository.GetFavoriteAsync(userId, facilityId);
            if (existingFavorite != null)
            {
                // Nếu đã yêu thích, xóa yêu thích
                await _favoriteRepository.DeleteAsync(existingFavorite);
                return Json(new { isFavorite = false });
            }
            else
            {
                // Nếu chưa yêu thích, thêm yêu thích
                var favorite = new Favorite
                {
                    UserId = userId,
                    FacilityId = facilityId
                };
                await _favoriteRepository.AddAsync(favorite);
                return Json(new { isFavorite = true });
            }
        }
        public async Task<IActionResult> Favorites()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorites = await _favoriteRepository.GetFavoritesByUserIdAsync(userId);
            if(favorites == null || !favorites.Any())
            {
                return View(new List<FacilityCardViewModel>());
            }
            var model = favorites.Select(f => new FacilityCardViewModel
            {
                Id = f.Id,
                Name = f.Name,
                Address = f.Address,
                OpeningTime = f.OpeningTime,
                ClosingTime = f.ClosingTime,
                AverageRating = _reviewRepository.AverageRating(f.Id).Result,
                ImageUrl = _facilityRepository.GetMainImage(f.Id).Result ?? "/images/default-facility.jpg",
                IsFavorite = _favoriteRepository.IsFavoriteAsync(userId, f.Id).Result
            }).ToList();
            return View(model);
        }
        public async Task<IActionResult> BookingHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);

            var bookingViewModels = bookings.Select(booking => new BookingHistoryViewModel
            {
                Id = booking.Id,
                FieldName = booking.Field.Name,
                FacilityName = booking.Field.Facility.Name,
                FieldTypeName = booking.Field.Type.GetDisplayName(),
                BookingDate = booking.BookingDate,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Status = booking.Status.ToString(),
                TotalPrice = booking.TotalPrice,
                Notes = "", // Add notes if needed in the future
                CanCancel = DateTime.Now < booking.BookingDate.AddDays(-1) && booking.Status != BookingStatus.Cancelled,  // Kiểm tra nếu thời gian còn hơn 1 ngày
                CanReview = booking.Status == BookingStatus.Completed && DateTime.Now > booking.BookingDate.Add(booking.StartTime) && booking.isReviewed == false  // Nếu trạng thái là hoàn thành, cho phép đánh giá
            }).ToList();

            return View(bookingViewModels);
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

            await _hubContext.Clients.Group("Admin")
                .SendAsync("ReceiveNotification", $"❌ Booking #{booking.Id} đã bị hủy");

            await _hubContext.Clients.Group("FieldOwner")
                .SendAsync("ReceiveNotification", $"❌ Booking #{booking.Id} bị hủy");


            return RedirectToAction("BookingHistory");
        }

        public async Task<IActionResult> Review(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound("Booking không tồn tại.");
            }
            var field = await _fieldRepository.GetByIdAsync(booking.FieldId);
            var facility = await _facilityRepository.GetByIdAsync(field.FacilityId);
            if (facility == null)
            {
                return NotFound("Cơ sở không tồn tại.");
            }
            ViewBag.bookingId = bookingId;
            return View(facility);
        }

        [HttpPost]
        public async Task<IActionResult> Review(int facilityId, int rating, string comment, int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Home") });
            }
            var review = new Review
            {
                UserId = userId,
                FacilityId = facilityId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };
            await _reviewRepository.AddAsync(review);
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking != null)
            {
                booking.isReviewed = true;
                await _bookingRepository.UpdateAsync(booking);
            }
            return RedirectToAction("BookingHistory", "Home");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult TestSignalR()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
