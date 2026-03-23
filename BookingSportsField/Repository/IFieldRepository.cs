using BookingSportsField.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Repository
{
    public interface IFieldRepository : IRepository<Field>
    {
        Task<Field> GetByIdAsync(int id);
        Task<List<Field>> GetFieldsByFacilityIdAsync(int facilityId);
        Task<List<Field>> GetAvailableFields(int facilityId, DateTime date, int startHour, int endHour);
    }
}
