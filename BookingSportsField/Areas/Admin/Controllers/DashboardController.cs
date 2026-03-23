using BookingSportsField.Models;
using BookingSportsField.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSportsField.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {

        private readonly IFacilityRepository _facilityRepository;
        public DashboardController(IFacilityRepository facilityRepository)
        {
            _facilityRepository = facilityRepository;
        }
        public async Task<IActionResult> Index()
        {
            var facilities = await _facilityRepository.GetAllAsync();
            var pendingCount = facilities.Count(f => f.ApprovalStatus == ApprovalStatus.Pending);
            ViewBag.PendingFacilityCount = pendingCount;
            return View();
        }

    }
}
