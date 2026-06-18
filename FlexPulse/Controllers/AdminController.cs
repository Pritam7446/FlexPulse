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
        var users = _userManager.Users.ToList();
        var list = new List<AdminUserViewModel>();
        foreach (var u in users)
        {
            var vm = new AdminUserViewModel { Id = u.Id, Email = u.Email, UserName = u.UserName };
            var claims = _userManager.GetClaimsAsync(u).GetAwaiter().GetResult();
            var full = claims.FirstOrDefault(c => c.Type == "FullName")?.Value;
            if (!string.IsNullOrWhiteSpace(full)) vm.UserName = full; // show full name instead of username when available
            list.Add(vm);
        }

        return View(list);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var details = new AdminUserDetailsViewModel
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName
        };

        var claims = await _userManager.GetClaimsAsync(user);
        foreach (var c in claims)
        {
            if (c.Type == "FullName") details.FullName = c.Value;
            else if (c.Type == "DateOfBirth" && DateTime.TryParse(c.Value, out var d)) details.DateOfBirth = d;
            else if (c.Type == "Gender") details.Gender = c.Value;
            else if (c.Type == System.Security.Claims.ClaimTypes.MobilePhone) details.MobileNumber = c.Value;
            details.Claims.Add(new KeyValuePair<string, string>(c.Type, c.Value));
        }

        details.Roles = (await _userManager.GetRolesAsync(user)).ToList();

        return View(details);
    }

    // Removed RemoveClaim action - claims UI removed

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

        // Load claims for profile fields
        var claims = await _userManager.GetClaimsAsync(user);
        model.FullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value;
        if (DateTime.TryParse(claims.FirstOrDefault(c => c.Type == "DateOfBirth")?.Value, out var dob)) model.DateOfBirth = dob;
        model.Gender = claims.FirstOrDefault(c => c.Type == "Gender")?.Value;
        model.MobileNumber = claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.MobilePhone)?.Value;

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

        // Update profile claims
        var existing = await _userManager.GetClaimsAsync(user);
        var claimOps = new List<System.Security.Claims.Claim>();

        // Helper to replace claim
        async Task ReplaceClaim(string type, string? value)
        {
            var old = existing.FirstOrDefault(c => c.Type == type);
            if (old != null) await _userManager.RemoveClaimAsync(user, old);
            if (!string.IsNullOrWhiteSpace(value)) await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(type, value));
        }

        await ReplaceClaim("FullName", model.FullName);
        await ReplaceClaim("DateOfBirth", model.DateOfBirth?.ToString("o"));
        await ReplaceClaim("Gender", model.Gender);
        await ReplaceClaim(System.Security.Claims.ClaimTypes.MobilePhone, model.MobileNumber);

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


