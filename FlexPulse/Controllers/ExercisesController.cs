using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlexPulse.Data;
using FlexPulse.Models;
using Microsoft.AspNetCore.Authorization;

namespace FlexPulse.Controllers;

[Authorize(Roles = "Admin")]
public class ExercisesController : Controller
{
    private readonly ApplicationDbContext _db;

    public ExercisesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Exercises.OrderBy(e => e.Name).ToListAsync();
        return View(list);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var ex = await _db.Exercises.FindAsync(id.Value);
        if (ex == null) return NotFound();
        return View(ex);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,MuscleGroup,Equipment,CaloriesPerMinute")] Exercise exercise)
    {
        if (ModelState.IsValid)
        {
            _db.Exercises.Add(exercise);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(exercise);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var ex = await _db.Exercises.FindAsync(id.Value);
        if (ex == null) return NotFound();
        return View(ex);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,MuscleGroup,Equipment,CaloriesPerMinute")] Exercise exercise)
    {
        if (id != exercise.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                _db.Update(exercise);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Exercises.AnyAsync(e => e.Id == exercise.Id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(exercise);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var ex = await _db.Exercises.FindAsync(id.Value);
        if (ex == null) return NotFound();
        return View(ex);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ex = await _db.Exercises.FindAsync(id);
        if (ex != null)
        {
            _db.Exercises.Remove(ex);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
