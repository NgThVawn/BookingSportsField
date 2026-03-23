using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BookingSportsField.Models
{
    public enum FieldType
    {
        [Display(Name = "Sân 5 cỏ nhân tạo")]
        FiveArtificial,
        [Display(Name = "Sân 7 cỏ nhân tạo")]
        SevenArtificial,
        [Display(Name = "Sân 5 trong nhà")]
        FiveIndoor
    }
}
