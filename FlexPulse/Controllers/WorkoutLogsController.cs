using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FlexPulse.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlexPulse.Controllers;

[Authorize]
public class WorkoutLogsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public WorkoutLogsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var logs = await _db.WorkoutLogs.Include(w => w.Exercise).Where(w => w.UserId == user.Id).ToListAsync();
        return View(logs);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Exercises"] = await _db.Exercises.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FlexPulse.Models.WorkoutLog model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (!ModelState.IsValid)
        {
            ViewData["Exercises"] = await _db.Exercises.ToListAsync();
            return View(model);
        }

        model.UserId = user.Id;
        _db.WorkoutLogs.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
