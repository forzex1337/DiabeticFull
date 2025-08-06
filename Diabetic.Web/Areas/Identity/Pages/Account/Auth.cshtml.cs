using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Diabetic.Shared.Models;

namespace Diabetic.Web.Areas.Identity.Pages.Account;

public class AuthModel : PageModel
{
    private readonly SignInManager<DiabeticUser> _signInManager;
    private readonly UserManager<DiabeticUser> _userManager;
    private readonly IUserStore<DiabeticUser> _userStore;
    private readonly ILogger<AuthModel> _logger;

    public AuthModel(
        UserManager<DiabeticUser> userManager,
        IUserStore<DiabeticUser> userStore,
        SignInManager<DiabeticUser> signInManager,
        ILogger<AuthModel> logger)
    {
        _userManager = userManager;
        _userStore = userStore;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public LoginInputModel LoginInput { get; set; } = new();

    [BindProperty]
    public RegisterInputModel RegisterInput { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public bool IsRegisterAttempt { get; set; } = false;

    public class LoginInputModel
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format email.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Zapamiętaj mnie")]
        public bool RememberMe { get; set; }
    }

    public class RegisterInputModel
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format email.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2} i maksymalnie {1} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasła nie są identyczne.")]
        public string ConfirmPassword { get; set; } = "";

        [Display(Name = "Imię")]
        public string? FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Typ cukrzycy jest wymagany.")]
        [Display(Name = "Typ cukrzycy")]
        public string DiabetesType { get; set; } = "";

        [Display(Name = "Data urodzenia")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Płeć")]
        public string? Gender { get; set; }

        [Display(Name = "Wzrost (cm)")]
        [Range(50, 300, ErrorMessage = "Wzrost musi być między 50 a 300 cm.")]
        public double? Height { get; set; }

        [Display(Name = "Waga (kg)")]
        [Range(10, 500, ErrorMessage = "Waga musi być między 10 a 500 kg.")]
        public double? Weight { get; set; }

        [Display(Name = "Numer telefonu")]
        [Phone(ErrorMessage = "Nieprawidłowy numer telefonu.")]
        public string? PhoneNumber { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostLoginAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        IsRegisterAttempt = false;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(LoginInput.Email, LoginInput.Password, LoginInput.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = LoginInput.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nieprawidłowe dane logowania.");
                return Page();
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        IsRegisterAttempt = true;

        if (ModelState.IsValid)
        {
            var user = new DiabeticUser
            {
                UserName = RegisterInput.Email,
                Email = RegisterInput.Email,
                FirstName = RegisterInput.FirstName,
                LastName = RegisterInput.LastName,
                DiabetesType = RegisterInput.DiabetesType,
                DateOfBirth = RegisterInput.DateOfBirth,
                Gender = RegisterInput.Gender,
                Height = RegisterInput.Height,
                Weight = RegisterInput.Weight,
                PhoneNumber = RegisterInput.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userStore.SetUserNameAsync(user, RegisterInput.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, RegisterInput.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

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

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }
}