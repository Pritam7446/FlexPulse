using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FlexPulse.Models;

namespace FlexPulse.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<WorkoutLog> WorkoutLogs { get; set; } = null!;
}
