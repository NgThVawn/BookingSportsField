using BookingSportsField.Models;

namespace BookingSportsField.Repository
{
    public interface IFacilityRepository : IRepository<Facility>
    {
        Task<Facility> GetByIdWithImagesAsync(int id);
        Task<List<Facility>> GetAsync();
        Task<string> GetMainImage(int id);
        Task<List<Facility>> GetByOwnerIdAsync(string ownerId);
        Task<List<Facility>> GetActiveAsync();
    }
}
