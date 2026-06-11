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

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var log = await _db.WorkoutLogs.FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);
        if (log == null) return NotFound();

        ViewData["Exercises"] = await _db.Exercises.ToListAsync();
        return View(log);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FlexPulse.Models.WorkoutLog model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewData["Exercises"] = await _db.Exercises.ToListAsync();
            return View(model);
        }

        var existing = await _db.WorkoutLogs.FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);
        if (existing == null) return NotFound();

        // Update allowed fields
        existing.ExerciseId = model.ExerciseId;
        existing.Date = model.Date;
        existing.DurationMinutes = model.DurationMinutes;

        // Recalculate calories using exercise metadata
        var exercise = await _db.Exercises.FindAsync(model.ExerciseId);
        if (exercise != null)
        {
            existing.CaloriesBurned = (int)Math.Round(exercise.CaloriesPerMinute * model.DurationMinutes);
        }

        _db.WorkoutLogs.Update(existing);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var logs = await _db.WorkoutLogs.Include(w => w.Exercise).Where(w => w.UserId == user.Id).ToListAsync();
        return View(logs);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var log = await _db.WorkoutLogs.Include(w => w.Exercise).FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);
        if (log == null) return NotFound();

        return View(log);
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

        // Calculate calories burned from exercise metadata if available
        var exercise = await _db.Exercises.FindAsync(model.ExerciseId);
        if (exercise != null)
        {
            model.CaloriesBurned = (int)Math.Round(exercise.CaloriesPerMinute * model.DurationMinutes);
        }

        _db.WorkoutLogs.Add(model);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var log = await _db.WorkoutLogs.FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);
        if (log == null) return NotFound();

        _db.WorkoutLogs.Remove(log);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
