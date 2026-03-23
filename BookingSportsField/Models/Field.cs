namespace BookingSportsField.Models
{
    public class Field
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public FieldType Type { get; set; }
        public int FacilityId { get; set; }
        public bool IsActive { get; set; } = true;
        public Facility Facility { get; set; }
        public decimal PricePerHour { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
