using BookingSportsField.Models;

namespace BookingSportsField.ViewModels
{
    public class UserWithRolesViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
        public bool IsLocked { get; set; }
    }
}
