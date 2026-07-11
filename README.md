# FlexPulse

FlexPulse is a lightweight fitness tracking web application built with .NET 8 using a hybrid of Razor Pages and ASP.NET Core MVC. It includes user authentication (ASP.NET Core Identity), exercise metadata, and workout logging.

Key points
- Targets: .NET 8
- UI: Razor Pages and MVC views (hybrid)
- Data access: Entity Framework Core with SQL Server / LocalDB
- Authentication: ASP.NET Core Identity (roles supported)

Important project notes
- The application seeds demo data on startup (see `Data/DbInitializer.cs`).
- Email-based flows (forgot password, reset password, email confirmation) have been removed. No IEmailSender is registered.
- After saving a workout log (creating a run), the user is automatically signed out.

Getting started
Prerequisites
- .NET 8 SDK
- (Optional) Visual Studio 2022/2026 or VS Code

Run locally
1. Configure the connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection` if you want to use a different SQL Server instance. By default the project uses a SQL Server connection named `DefaultConnection`.
2. From the project folder run:

   dotnet build
   dotnet ef database update    # applies migrations
   dotnet run

3. Open the site (usually at https://localhost:5001) and sign in.



Admin area
- AdminController provides a simple admin dashboard at `/Admin` (requires Admin role).
- An Admin navigation link is shown in the main layout when a user is in the Admin role.
- Exercise management:
  - Exercises list: `/Exercises` (public)
  - Admin can edit an exercise via the Razor Page: `/Admin/Exercises/Edit/{id}` or by the Edit button on the Exercises list when signed in as Admin.

Workout logs
- Users can add workout logs at `/WorkoutLogs/Create`.
- When a workout log is saved, the app will automatically sign the user out and redirect to the Home page.

Database and migrations
- EF Core migrations are included in the project. To add a migration and update the DB:

   dotnet ef migrations add <Name>
   dotnet ef database update

Promoting a user to Admin
If you need to promote an existing user to Admin, one option is to run SQL against the app database (use caution):

1. Find the user id and the role id (Admin) in the AspNetUsers / AspNetRoles tables.
2. Insert a record into AspNetUserRoles:

   INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('<user-id>', '<admin-role-id>');

Alternatively, add a small temporary endpoint or script that calls UserManager.AddToRoleAsync(user, "Admin") and run it once.

Customization and development notes
- The project intentionally disables email-based password reset/confirmation flows. Re-enabling requires adding an IEmailSender implementation and restoring the Identity UI pages for those flows.
- The application seeds a canonical set of exercises on startup. Modify `Data/DbInitializer.cs` to change seeds.

License
- This repository does not include a license file. Add one if you intend to publish or distribute the code.
