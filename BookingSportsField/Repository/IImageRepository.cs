using BookingSportsField.Models;

namespace BookingSportsField.Repository
{
    public interface IImageRepository : IRepository<FacilityImage>
    {
        Task<ICollection<FacilityImage>> GetImagesByFacilityIdAsync(int facilityId);
    }
}
