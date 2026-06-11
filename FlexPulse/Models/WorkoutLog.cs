using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FlexPulse.Models;

public class WorkoutLog
{
    public int Id { get; set; }

    // UserId is assigned server-side from the authenticated user; do not bind from the client form
    [BindNever]
    public string? UserId { get; set; }

    [Required]
    public int ExerciseId { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    public Exercise? Exercise { get; set; }

    public DateTime Date { get; set; }

    public int DurationMinutes { get; set; }

    public int CaloriesBurned { get; set; }
}
