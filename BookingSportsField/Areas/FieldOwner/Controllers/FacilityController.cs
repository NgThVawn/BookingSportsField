using BookingSportsField.Models;
using BookingSportsField.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookingSportsField.Areas.FieldOwner.Controllers
{
    [Area("FieldOwner")]
    [Authorize(Roles = "FieldOwner")]
    public class FacilityController : Controller
    {
        private readonly IFacilityRepository _facilityRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<FacilityImage> _imageRepository;
        private readonly IReviewRepository _reviewRepository;
        public FacilityController(IFacilityRepository facilityRepository, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, IRepository<FacilityImage> imageRepository, IReviewRepository reviewRepository)
        {
            _facilityRepository = facilityRepository;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _imageRepository = imageRepository;
            _reviewRepository = reviewRepository;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var facilities = await _facilityRepository.GetByOwnerIdAsync(user.Id);
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
            facility.ApprovalStatus = ApprovalStatus.Pending;
            facility.IsActive = false;

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
            else if(facility.ApprovalStatus != ApprovalStatus.Accepted)
            {
                TempData["ErrorMessage"] = "Cơ sở của bạn cần được kiểm duyệt trước khi chỉnh sửa!";
                return RedirectToAction("Index");
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
            existingFacility.IsActive = facility.IsActive;
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
    }
}
