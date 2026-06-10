using Microsoft.EntityFrameworkCore;
using FlexPulse.Models;
using Microsoft.AspNetCore.Identity;

namespace FlexPulse.Data;

public static class DbInitializer
{
    public static void Initialize(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var scoped = scope.ServiceProvider;
        var context = scoped.GetRequiredService<ApplicationDbContext>();
        var userManager = scoped.GetRequiredService<UserManager<IdentityUser>>();

        context.Database.Migrate();

        if (context.Exercises.Any())
            return; // already seeded

        var exercises = new List<Exercise>
        {
            new Exercise { Name = "Squat", MuscleGroup = "Legs", Equipment = "Barbell" },
            new Exercise { Name = "Bench Press", MuscleGroup = "Chest", Equipment = "Barbell" },
            new Exercise { Name = "Pull Up", MuscleGroup = "Back", Equipment = "Bodyweight" }
        };

        context.Exercises.AddRange(exercises);
        context.SaveChanges();

        // Create a demo Identity user
        var demoEmail = "demo@flexpulse.local";
        var demoUser = userManager.FindByEmailAsync(demoEmail).GetAwaiter().GetResult();
        if (demoUser == null)
        {
            demoUser = new IdentityUser { UserName = demoEmail, Email = demoEmail, EmailConfirmed = true };
            var result = userManager.CreateAsync(demoUser, "P@ssw0rd!").GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create demo user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        var logs = new List<WorkoutLog>
        {
            new WorkoutLog { UserId = demoUser.Id, ExerciseId = exercises[0].Id, Date = DateTime.UtcNow.Date.AddDays(-2), DurationMinutes = 45, CaloriesBurned = 350 },
            new WorkoutLog { UserId = demoUser.Id, ExerciseId = exercises[1].Id, Date = DateTime.UtcNow.Date.AddDays(-1), DurationMinutes = 30, CaloriesBurned = 250 },
            new WorkoutLog { UserId = demoUser.Id, ExerciseId = exercises[2].Id, Date = DateTime.UtcNow.Date, DurationMinutes = 20, CaloriesBurned = 150 }
        };

        context.WorkoutLogs.AddRange(logs);
        context.SaveChanges();
    }
}
