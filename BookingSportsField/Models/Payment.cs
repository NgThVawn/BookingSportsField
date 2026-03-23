namespace BookingSportsField.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public bool isPaid { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
