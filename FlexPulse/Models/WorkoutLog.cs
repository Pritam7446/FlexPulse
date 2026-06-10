using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlexPulse.Models;

public class WorkoutLog
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!; // For Identity linkage

    [Required]
    public int ExerciseId { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    public Exercise? Exercise { get; set; }

    public DateTime Date { get; set; }

    public int DurationMinutes { get; set; }

    public int CaloriesBurned { get; set; }
}
