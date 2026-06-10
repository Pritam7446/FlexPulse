using FlexPulse.Models;
namespace FlexPulse.ViewModels;

public class DashboardViewModel
{
    public int TotalSessions { get; set; }
    public int ActiveMinutes { get; set; }
    public int Calories { get; set; }
    public List<Exercise> Exercises { get; set; } = new();
}
