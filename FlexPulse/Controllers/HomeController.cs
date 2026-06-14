using FlexPulse.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace FlexPulse.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FlexPulse.Data.ApplicationDbContext _db;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, FlexPulse.Data.ApplicationDbContext db, Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            int totalSessions = 0;
            int activeMinutes = 0;
            int calories = 0;

            if (User?.Identity?.IsAuthenticated ?? false)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var userLogs = _db.WorkoutLogs.Where(w => w.UserId == user.Id);
                    totalSessions = await userLogs.CountAsync();
                    activeMinutes = await userLogs.SumAsync(w => (int?)w.DurationMinutes) ?? 0;
                    calories = await userLogs.SumAsync(w => (int?)w.CaloriesBurned) ?? 0;
                }
            }
            else
            {
                // anonymous: do not show any personal or global data
                totalSessions = 0;
                activeMinutes = 0;
                calories = 0;
            }

            var vm = new FlexPulse.ViewModels.DashboardViewModel
            {
                TotalSessions = totalSessions,
                ActiveMinutes = activeMinutes,
                Calories = calories,
                Exercises = await _db.Exercises.ToListAsync()
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
