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

        // Seed a canonical set of exercises without duplicating existing entries.
        var seedExercises = new List<Exercise>
        {
            new Exercise { Name = "Squat", MuscleGroup = "Legs", Equipment = "Barbell", CaloriesPerMinute = 8.0 },
            new Exercise { Name = "Bench Press", MuscleGroup = "Chest", Equipment = "Barbell", CaloriesPerMinute = 7.0 },
            new Exercise { Name = "Pull Up", MuscleGroup = "Back", Equipment = "Bodyweight", CaloriesPerMinute = 6.5 },
            new Exercise { Name = "Deadlift", MuscleGroup = "Back", Equipment = "Barbell", CaloriesPerMinute = 8.5 },
            new Exercise { Name = "Overhead Press", MuscleGroup = "Shoulders", Equipment = "Barbell", CaloriesPerMinute = 6.8 },
            new Exercise { Name = "Lunge", MuscleGroup = "Legs", Equipment = "Bodyweight", CaloriesPerMinute = 6.0 },
            new Exercise { Name = "Bicep Curl", MuscleGroup = "Arms", Equipment = "Dumbbell", CaloriesPerMinute = 4.2 },
            new Exercise { Name = "Tricep Dip", MuscleGroup = "Arms", Equipment = "Bodyweight", CaloriesPerMinute = 5.0 },
            new Exercise { Name = "Plank", MuscleGroup = "Core", Equipment = "Bodyweight", CaloriesPerMinute = 3.5 },
            new Exercise { Name = "Burpee", MuscleGroup = "Full Body", Equipment = "Bodyweight", CaloriesPerMinute = 10.0 },
            new Exercise { Name = "Rowing", MuscleGroup = "Back", Equipment = "Machine", CaloriesPerMinute = 9.0 },
            new Exercise { Name = "Cycling", MuscleGroup = "Legs", Equipment = "Machine", CaloriesPerMinute = 7.5 }
        };

        var existingList = context.Exercises.ToList();
        var changedSeed = false;

        foreach (var seed in seedExercises)
        {
            var match = existingList.FirstOrDefault(e => string.Equals(e.Name?.Trim(), seed.Name, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                context.Exercises.Add(seed);
                existingList.Add(seed);
                changedSeed = true;
            }
            else
            {
                // If existing entry has no calories set, update it from seed
                if (match.CaloriesPerMinute == 0.0 && seed.CaloriesPerMinute > 0)
                {
                    match.CaloriesPerMinute = seed.CaloriesPerMinute;
                    changedSeed = true;
                }
            }
        }

        if (changedSeed)
        {
            context.SaveChanges();
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

        var adminEmail = "admin@flexpulse.com";
        var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (adminUser == null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = userManager.CreateAsync(adminUser, "Bb3$dddd").GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Ensure admin has the seeded password (use reset token) - useful during development when DB already exists
            try
            {
                var token = userManager.GeneratePasswordResetTokenAsync(adminUser).GetAwaiter().GetResult();
                var resetResult = userManager.ResetPasswordAsync(adminUser, token, "Bb3$dddd").GetAwaiter().GetResult();
                // ignore failures in production; this is a development convenience
            }
            catch
            {
                // swallow exceptions - do not block app startup
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
