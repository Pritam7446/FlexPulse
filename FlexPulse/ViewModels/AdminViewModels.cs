namespace FlexPulse.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = null!;
        public string? Email { get; set; }
        public string? UserName { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = null!;
        public string? Email { get; set; }
        public List<string>? Roles { get; set; }
        public List<string> AllRoles { get; set; } = new();
    }
}
