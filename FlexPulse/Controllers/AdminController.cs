using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FlexPulse.ViewModels;

namespace FlexPulse.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        var users = _userManager.Users.Select(u => new AdminUserViewModel
        {
            Id = u.Id,
            Email = u.Email,
            UserName = u.UserName
        }).ToList();

        return View(users);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            Roles = (await _userManager.GetRolesAsync(user)).ToList()
        };

        model.AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);
        var selected = model.Roles ?? new List<string>();

        var toAdd = selected.Except(userRoles).ToArray();
        var toRemove = userRoles.Except(selected).ToArray();

        if (toAdd.Any()) await _userManager.AddToRolesAsync(user, toAdd);
        if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(user, toRemove);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        await _userManager.DeleteAsync(user);
        return RedirectToAction(nameof(Index));
    }

}


