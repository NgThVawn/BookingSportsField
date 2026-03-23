namespace BookingSportsField.ViewModels
{
    public class EditUserRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public List<string> AllRoles { get; set; }
        public string SelectedRole { get; set; }
    }
}
