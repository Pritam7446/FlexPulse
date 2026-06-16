using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace FlexPulse.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = null!;

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            // Determine requested destination type
            var isAdminReturn = !string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase);
            var isMemberReturn = !string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/WorkoutLogs", StringComparison.OrdinalIgnoreCase);

            // Check role constraints before signing in when we can (do not reveal existence otherwise)
            var precheckUser = await _userManager.FindByEmailAsync(Input.Email);
            if (precheckUser != null)
            {
                var preIsAdmin = await _userManager.IsInRoleAsync(precheckUser, "Admin");
                if (preIsAdmin && isMemberReturn)
                {
                    // Admins should use Admin Login
                    var msg = "Admin accounts must use the Admin Login button.";
                    ModelState.AddModelError(string.Empty, msg);
                    TempData["AuthError"] = msg;
                    ViewData["AuthError"] = msg;
                    _logger?.LogWarning("Admin attempted member login: {Email}", Input.Email);
                    return Page();
                }

                if (!preIsAdmin && isAdminReturn)
                {
                    // Non-admins cannot sign into admin area
                    var msg = "You do not have permissions to access the admin area.";
                    ModelState.AddModelError(string.Empty, msg);
                    TempData["AuthError"] = msg;
                    ViewData["AuthError"] = msg;
                    _logger?.LogWarning("Non-admin attempted admin login: {Email}", Input.Email);
                    return Page();
                }
            }

            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
            _logger?.LogInformation("Login attempt for {Email}. Succeeded={Succeeded} IsLockedOut={IsLockedOut} RequiresTwoFactor={RequiresTwoFactor} IsNotAllowed={IsNotAllowed}", Input.Email, result.Succeeded, result.IsLockedOut, result.RequiresTwoFactor, result.IsNotAllowed);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            var invalid = "Invalid login attempt.";
            ModelState.AddModelError(string.Empty, invalid);
            TempData["AuthError"] = invalid;
            ViewData["AuthError"] = invalid;
            _logger?.LogWarning("Invalid login for {Email}", Input.Email);
        }

        return Page();
    }
}
