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

        if (!context.Exercises.Any())
        {
            var exercises = new List<Exercise>
            {
                new Exercise { Name = "Squat", MuscleGroup = "Legs", Equipment = "Barbell", CaloriesPerMinute = 8.0 },
                new Exercise { Name = "Bench Press", MuscleGroup = "Chest", Equipment = "Barbell", CaloriesPerMinute = 7.0 },
                new Exercise { Name = "Pull Up", MuscleGroup = "Back", Equipment = "Bodyweight", CaloriesPerMinute = 6.5 }
            };

            context.Exercises.AddRange(exercises);
            context.SaveChanges();
        }
        else
        {
            // If the CaloriesPerMinute column was added after initial seeding, update existing rows with sensible defaults if unset
            var nameMap = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "Squat", 8.0 },
                { "Bench Press", 7.0 },
                { "Pull Up", 6.5 }
            };

            var existing = context.Exercises.Where(e => e.CaloriesPerMinute == 0.0).ToList();
            var changed = false;
            foreach (var ex in existing)
            {
                if (nameMap.TryGetValue(ex.Name, out var val))
                {
                    ex.CaloriesPerMinute = val;
                    changed = true;
                }
            }
            if (changed) context.SaveChanges();
        }

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

        // Ensure roles and assign demo user to Member role
        var roleManager = scoped.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
        var memberRole = "Member";
        if (!roleManager.RoleExistsAsync(memberRole).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(memberRole)).GetAwaiter().GetResult();
        }
        if (!userManager.IsInRoleAsync(demoUser, memberRole).GetAwaiter().GetResult())
        {
            userManager.AddToRoleAsync(demoUser, memberRole).GetAwaiter().GetResult();
        }

        // Ensure Admin role and create an admin user
        var adminRole = "Admin";
        if (!roleManager.RoleExistsAsync(adminRole).GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(adminRole)).GetAwaiter().GetResult();
        }

        var adminEmail = "admin@flexpulse.local";
        var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (adminUser == null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = userManager.CreateAsync(adminUser, "AdminP@ssw0rd!").GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        if (!userManager.IsInRoleAsync(adminUser, adminRole).GetAwaiter().GetResult())
        {
            userManager.AddToRoleAsync(adminUser, adminRole).GetAwaiter().GetResult();
        }

        // Seed demo logs referencing exercises from the database
        var dbExercises = context.Exercises.OrderBy(e => e.Id).Take(3).ToList();
        if (dbExercises.Count >= 3)
        {
            var logs = new List<WorkoutLog>
            {
                new WorkoutLog { UserId = demoUser.Id, ExerciseId = dbExercises[0].Id, Date = DateTime.UtcNow.Date.AddDays(-2), DurationMinutes = 45, CaloriesBurned = (int)Math.Round(dbExercises[0].CaloriesPerMinute * 45) },
                new WorkoutLog { UserId = demoUser.Id, ExerciseId = dbExercises[1].Id, Date = DateTime.UtcNow.Date.AddDays(-1), DurationMinutes = 30, CaloriesBurned = (int)Math.Round(dbExercises[1].CaloriesPerMinute * 30) },
                new WorkoutLog { UserId = demoUser.Id, ExerciseId = dbExercises[2].Id, Date = DateTime.UtcNow.Date, DurationMinutes = 20, CaloriesBurned = (int)Math.Round(dbExercises[2].CaloriesPerMinute * 20) }
            };

            context.WorkoutLogs.AddRange(logs);
            context.SaveChanges();
        }
    }
}
