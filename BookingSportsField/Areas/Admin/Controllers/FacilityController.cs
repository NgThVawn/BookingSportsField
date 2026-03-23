using BookingSportsField.Models;
using BookingSportsField.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookingSportsField.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FacilityController : Controller
    {
        private readonly IFacilityRepository _facilityRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<FacilityImage> _imageRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IReviewRepository _reviewRepository;

        public FacilityController(IFacilityRepository facilityRepository, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, IRepository<FacilityImage> imageRepository, IBookingRepository bookingRepository, IReviewRepository reviewRepository)
        {
            _facilityRepository = facilityRepository;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _imageRepository = imageRepository;
            _bookingRepository = bookingRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<IActionResult> Index()
        {
            var facilities = await _facilityRepository.GetAsync();
            return View(facilities);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Facility facility)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            facility.OwnerId = user.Id;
            facility.Images = new List<FacilityImage>();

            if (facility.UploadImages != null && facility.UploadImages.Any())
            {
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                Directory.CreateDirectory(uploadsPath);

                foreach (var file in facility.UploadImages)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        facility.Images.Add(new FacilityImage
                        {
                            ImageUrl = "/images/" + fileName
                        });
                    }
                }
            }
            await _facilityRepository.AddAsync(facility);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Edit(int id)
        {
            var facility = await _facilityRepository.GetByIdWithImagesAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            return View(facility);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Facility facility)
        {
            if (id != facility.Id)
            {
                return NotFound();
            }

            var existingFacility = await _facilityRepository.GetByIdWithImagesAsync(id);
            if (existingFacility == null)
            {
                return NotFound();
            }

            existingFacility.Name = facility.Name;
            existingFacility.Address = facility.Address;
            existingFacility.OpeningTime = facility.OpeningTime;
            existingFacility.ClosingTime = facility.ClosingTime;
            existingFacility.OwnerId = facility.OwnerId;
            // Xử lý ảnh
            if (facility.UploadImages != null && facility.UploadImages.Any())
            {
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                Directory.CreateDirectory(uploadsPath);

                // Xóa ảnh cũ
                if (existingFacility.Images != null && existingFacility.Images.Any())
                {
                    foreach (var image in existingFacility.Images.ToList())
                    {
                        var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));

                        // Xóa file vật lý trong wwwroot/images
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                        await _imageRepository.DeleteAsync(image);
                    }
                }

                // Thêm ảnh mới
                foreach (var file in facility.UploadImages)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadsPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        existingFacility.Images.Add(new FacilityImage { ImageUrl = "/images/" + fileName });
                    }
                }
            }
            await _facilityRepository.UpdateAsync(existingFacility);
            return RedirectToAction("Index");
        }



        public async Task<IActionResult> Delete(int id)
        {
            var facility = await _facilityRepository.GetByIdAsync(id);
            if (facility == null)
            {
                return NotFound();
            }

            return View(facility);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var facility = await _facilityRepository.GetByIdWithImagesAsync(id);
            if (facility == null)
            {
                return NotFound();
            }
            if (facility.Images != null && facility.Images.Any())
            {
                foreach (var image in facility.Images.ToList())
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));

                    // Xóa file vật lý trong wwwroot/images
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    await _imageRepository.DeleteAsync(image);
                }
            }
            //Xóa booking
            if (facility.Fields != null && facility.Fields.Any())
            {
                foreach (var field in facility.Fields.ToList())
                {
                    var bookings = await _bookingRepository.GetBookingsByFieldIdAsync(field.Id);
                    if (bookings != null && bookings.Any())
                    {
                        foreach (var booking in bookings)
                        {
                            await _bookingRepository.DeleteAsync(booking);
                        }
                    }
                }
            }
            await _facilityRepository.DeleteAsync(facility);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            var facility = await _facilityRepository.GetByIdWithImagesAsync(id);

            if (facility == null)
            {
                return NotFound();
            }
            facility.Reviews = await _reviewRepository.GetReviewsByFacilityIdAsync(id);
            return View(facility);
        }
        public async Task<IActionResult> Pending()
        {
            var facilities = await _facilityRepository.GetAsync();
            var pending = facilities.Where(f => f.ApprovalStatus == ApprovalStatus.Pending).ToList();
            return View(pending);
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int id)
        {
            var facility = await _facilityRepository.GetByIdAsync(id);
            if (facility != null)
            {
                facility.ApprovalStatus = ApprovalStatus.Accepted;
                facility.IsActive = true;
                await _facilityRepository.UpdateAsync(facility);
            }
            return RedirectToAction("Pending");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var facility = await _facilityRepository.GetByIdAsync(id);
            if (facility != null)
            {
                facility.ApprovalStatus = ApprovalStatus.Rejected;
                await _facilityRepository.UpdateAsync(facility);
            }

            return RedirectToAction("Pending");
        }
    }
}
