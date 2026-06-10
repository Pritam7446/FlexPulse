using System.ComponentModel.DataAnnotations;

namespace FlexPulse.Models;

public class Exercise
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? MuscleGroup { get; set; }

    public string? Equipment { get; set; }
}
