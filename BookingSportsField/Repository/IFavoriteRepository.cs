using BookingSportsField.Models;

namespace BookingSportsField.Repository
{
    public interface IFavoriteRepository : IRepository<Favorite>
    {
        Task<Favorite> GetFavoriteAsync(string userId, int facilityId);
        Task<List<Facility>> GetFavoritesByUserIdAsync(string userId);
        Task<bool> IsFavoriteAsync(string userId, int facilityId);
    }
}
