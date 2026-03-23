namespace BookingSportsField.ViewModels
{
    public class AdminStatisticsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalUsers { get; set; }

        public List<BookingStatusRatio> StatusRatios { get; set; }
        public List<DailyBookingCount> DailyBookings { get; set; }

        public string MostBookedFacilityName { get; set; }
        public int MostBookedFacilityCount { get; set; }

        public string TopUserEmail { get; set; }
        public int TopUserBookingCount { get; set; }
    }

    public class BookingStatusRatio
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class DailyBookingCount
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }
}
