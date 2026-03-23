namespace BookingSportsField.ViewModels
{
    public class FacilityCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public string OpeningHours => $"{OpeningTime:hh\\:mm} - {ClosingTime:hh\\:mm}";
        public string ImageUrl { get; set; }
        public decimal AverageRating { get; set; }
        public bool IsFavorite { get; set; }
    }
}