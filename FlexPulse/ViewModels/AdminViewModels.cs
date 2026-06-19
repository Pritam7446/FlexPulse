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
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? MobileNumber { get; set; }
    }

    public class AdminUserDetailsViewModel
    {
        public string Id { get; set; } = null!;
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? MobileNumber { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<KeyValuePair<string, string>> Claims { get; set; } = new();
        public List<AdminUserWorkoutLogViewModel> WorkoutLogs { get; set; } = new();
    }

    public class AdminUserWorkoutLogViewModel
    {
        public DateTime Date { get; set; }
        public string? ExerciseName { get; set; }
        public int DurationMinutes { get; set; }
        public int CaloriesBurned { get; set; }
    }
}
