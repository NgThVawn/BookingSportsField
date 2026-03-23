using BookingSportsField.ViewModels;

namespace BookingSportsField.Repository
{
    public interface IRecommendationService
    {
        Task<List<FacilityRecommendationVM>> RecommendAsync(string userId);
    }

}
