namespace BookingSportsField.ViewModels
{
    public class BookingViewModel
    {
        public int FieldId { get; set; }
        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
