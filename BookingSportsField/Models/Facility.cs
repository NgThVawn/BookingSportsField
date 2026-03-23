using System.ComponentModel.DataAnnotations.Schema;

namespace BookingSportsField.Models
{
    public class Facility
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public bool IsActive { get; set; } = false;
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
        public string OwnerId { get; set; }
        [NotMapped]
        public List<IFormFile>? UploadImages { get; set; }
        public virtual ApplicationUser FieldOwner { get; set; }
        public virtual ICollection<Field> Fields { get; set; }
        public virtual ICollection<FacilityImage> Images { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
