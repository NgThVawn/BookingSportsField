using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BookingSportsField.Models
{
    public enum BookingStatus
    {
        [Display(Name = "Chờ xác nhận")]
        Pending,
        [Display(Name = "Đã xác nhận")]
        Confirmed,
        [Display(Name = "Đã hủy")]
        Cancelled,
        [Display(Name = "Đã hoàn thành")]
        Completed,
        [Display(Name = "Không nhận sân")]
        NoShow
    }
}
