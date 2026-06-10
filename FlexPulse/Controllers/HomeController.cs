using FlexPulse.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FlexPulse.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FlexPulse.Data.ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, FlexPulse.Data.ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            var vm = new FlexPulse.ViewModels.DashboardViewModel
            {
                TotalSessions = _db.WorkoutLogs.Count(),
                ActiveMinutes = _db.WorkoutLogs.Sum(w => w.DurationMinutes),
                Calories = _db.WorkoutLogs.Sum(w => w.CaloriesBurned),
                Exercises = _db.Exercises.ToList()
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
