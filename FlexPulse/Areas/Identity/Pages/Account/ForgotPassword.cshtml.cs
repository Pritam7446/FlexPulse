using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;

namespace FlexPulse.Areas.Identity.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender, ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = null!;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToPage("ForgotPasswordConfirmation");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code = HttpUtility.UrlEncode(token), email = Input.Email },
            protocol: Request.Scheme);

        // Log the callback URL (useful during development)
        _logger?.LogInformation("Password reset link for {email}: {url}", Input.Email, callbackUrl);

        await _emailSender.SendEmailAsync(Input.Email, "Reset Password", $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");

        return RedirectToPage("ForgotPasswordConfirmation");
    }
}
