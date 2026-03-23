namespace BookingSportsField.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int FacilityId { get; set; }
        public Facility Facility { get; set; }
    }
}
