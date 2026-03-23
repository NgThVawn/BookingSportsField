using BookingSportsField.Models;

namespace BookingSportsField.Repository
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<ICollection<Review>> GetReviewsByFacilityIdAsync(int facilityId);
        Task<decimal> AverageRating(int facilityId);
    }
}
