using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FlexPulse.Data;
using FlexPulse.Models;

namespace FlexPulse.Pages.Admin.Exercises;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public Exercise Exercise { get; set; } = new Exercise();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var ex = await _db.Exercises.FindAsync(id.Value);
        if (ex == null) return NotFound();

        Exercise = ex;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!await _db.Exercises.AnyAsync(e => e.Id == Exercise.Id))
        {
            return NotFound();
        }

        try
        {
            _db.Update(Exercise);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Exercises.AnyAsync(e => e.Id == Exercise.Id))
            {
                return NotFound();
            }
            throw;
        }

        // Redirect back to the existing MVC Exercises index
        return Redirect("/Exercises");
    }
}
