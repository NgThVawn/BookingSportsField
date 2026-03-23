namespace BookingSportsField.ViewModels
{
    public class BookingHistoryViewModel
    {
        public int Id { get; set; }
        public string FieldName { get; set; }
        public string FacilityName { get; set; }
        public string FieldTypeName { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReview { get; set; }
        public string Notes { get; set; }
    }
}
