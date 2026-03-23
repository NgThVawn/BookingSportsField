using System.ComponentModel.DataAnnotations;

namespace BookingSportsField.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int FieldId { get; set; }
        public Field Field { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BookingDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public bool isReviewed { get; set; } = false;
        public ICollection<Payment> Payments { get; set; }
    }
}
