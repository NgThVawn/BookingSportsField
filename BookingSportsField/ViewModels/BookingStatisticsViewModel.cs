using BookingSportsField.Models;

namespace BookingSportsField.ViewModels
{
    public class BookingStatisticsViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? SelectedFacilityId { get; set; } // chọn cơ sở
        public List<Facility> Facilities { get; set; } // danh sách cơ sở

        public int TotalBookings { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalCancelled { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Booking> Bookings { get; set; } = new();
        public List<BookingChartViewModel> ChartData { get; set; } = new();
    }
}
