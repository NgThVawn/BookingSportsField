using BookingSportsField.Models;
using BookingSportsField.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingSportsField.Areas.FieldOwner.Controllers
{
    [Area("FieldOwner")]
    [Authorize(Roles = "FieldOwner")]
    public class FieldController : Controller
    {
        private readonly IFacilityRepository _facilityRepository;
        private readonly IFieldRepository _fieldRepository;

        public FieldController(IFacilityRepository facilityRepository, IFieldRepository fieldRepository)
        {
            _facilityRepository = facilityRepository;
            _fieldRepository = fieldRepository;
        }
        private List<SelectListItem> GetFieldTypeSelectList()
        {
            return Enum.GetValues(typeof(FieldType))
                       .Cast<FieldType>()
                       .Select(ft => new SelectListItem
                       {
                           Value = ft.ToString(),
                           Text = ft.GetDisplayName()
                       }).ToList();
        }
        public async Task<IActionResult> Index(int facilityId)
        {
            var facility = await _facilityRepository.GetByIdAsync(facilityId);
            if (facility == null) return NotFound();

            ViewBag.FacilityName = facility.Name;
            ViewBag.FacilityId = facility.Id;

            var fields = await _fieldRepository.GetFieldsByFacilityIdAsync(facilityId);

            return View(fields);
        }
        public IActionResult Create(int facilityId)
        {
            var field = new Field
            {
                FacilityId = facilityId
            };
            ViewBag.FieldTypes = GetFieldTypeSelectList();
            return View(field);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Field field)
        {
            await _fieldRepository.AddAsync(field);
            return RedirectToAction("Details", "Facility", new { id = field.FacilityId });
        }
        public async Task<IActionResult> Delete(int id)
        {
            var field = await _fieldRepository.GetByIdAsync(id);
            if (field == null)
            {
                return NotFound();
            }
            return View(field);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var field = await _fieldRepository.GetByIdAsync(id);
            if (field == null)
            {
                return NotFound();
            }
            return View(field);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Field field)
        {
            var existingField = await _fieldRepository.GetByIdAsync(field.Id);
            if (existingField == null)
            {
                return NotFound();
            }

            // Cập nhật các trường cần thiết
            existingField.FacilityId = field.FacilityId;
            existingField.Id = field.Id;
            existingField.Name = field.Name;
            existingField.Type = field.Type;
            existingField.PricePerHour = field.PricePerHour;
            existingField.IsActive = field.IsActive;

            await _fieldRepository.UpdateAsync(existingField);

            return RedirectToAction("Details", "Facility", new { id = field.FacilityId });
        }


        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var field = await _fieldRepository.GetByIdAsync(id);
            if (field == null)
            {
                return NotFound();
            }
            int facilityId = field.FacilityId;
            await _fieldRepository.DeleteAsync(field);
            return RedirectToAction("Details", "Facility", new { id = facilityId });
        }
    }
}
