namespace BookingSportsField.Models
{
    public class FacilityImage
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public virtual Facility Facility { get; set; }

        public string ImageUrl { get; set; }
    }
}
