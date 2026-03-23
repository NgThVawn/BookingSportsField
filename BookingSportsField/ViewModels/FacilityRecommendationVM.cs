namespace BookingSportsField.ViewModels
{
    public class FacilityRecommendationVM
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public decimal Score { get; set; }

        // Gợi ý sân tốt nhất trong cơ sở
        public int BestFieldId { get; set; }
        public string BestFieldName { get; set; }

        public TimeSpan RecommendedStart { get; set; }
        public TimeSpan RecommendedEnd { get; set; }
    }


}
