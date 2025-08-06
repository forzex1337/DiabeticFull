using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Diabetic.Shared.Models;

namespace Diabetic.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<DiabeticUser> _signInManager;
        private readonly UserManager<DiabeticUser> _userManager;
        private readonly IUserStore<DiabeticUser> _userStore;
        private readonly IUserEmailStore<DiabeticUser> _emailStore;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<DiabeticUser> signInManager,
            UserManager<DiabeticUser> userManager,
            IUserStore<DiabeticUser> userStore,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public string ProviderDisplayName { get; set; } = default!;

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = default!;

            [Required]
            [Display(Name = "Imię")]
            public string FirstName { get; set; } = default!;

            [Required]
            [Display(Name = "Nazwisko")]
            public string LastName { get; set; } = default!;

            [Required]
            [Display(Name = "Typ cukrzycy")]
            public string DiabetesType { get; set; } = default!;
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Błąd dostawcy zewnętrznego: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Błąd podczas ładowania informacji logowania zewnętrznego.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
                if (info.Principal.HasClaim(ClaimTypes.Email, info.Principal.FindFirstValue(ClaimTypes.Email)!))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)!,
                        FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "",
                        LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "",
                        DiabetesType = "Type2" // Default value
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Błąd podczas ładowania informacji logowania zewnętrznego podczas potwierdzania.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                
                // Set diabetic-specific properties
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.DiabetesType = Input.DiabetesType;

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        // Add user to BasicUser role by default
                        try
                        {
                            await _userManager.AddToRoleAsync(user, "BasicUser");
                            _logger.LogInformation("User added to BasicUser role.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to add user to BasicUser role.");
                        }

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebUtility.UrlEncode(code);

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
            ReturnUrl = returnUrl;
            return Page();
        }

        private DiabeticUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<DiabeticUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(DiabeticUser)}'. " +
                    $"Ensure that '{nameof(DiabeticUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<DiabeticUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<DiabeticUser>)_userStore;
        }
    }
}