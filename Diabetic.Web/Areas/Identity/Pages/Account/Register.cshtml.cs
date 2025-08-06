using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Diabetic.Shared.Models;

namespace Diabetic.Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<DiabeticUser> _signInManager;
    private readonly UserManager<DiabeticUser> _userManager;
    private readonly IUserStore<DiabeticUser> _userStore;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<DiabeticUser> userManager,
        IUserStore<DiabeticUser> userStore,
        SignInManager<DiabeticUser> signInManager,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _userStore = userStore;
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public class InputModel
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

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        
        if (ModelState.IsValid)
        {
            var user = new DiabeticUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                DiabetesType = Input.DiabetesType,
                DateOfBirth = Input.DateOfBirth,
                Gender = Input.Gender,
                Height = Input.Height,
                Weight = Input.Weight,
                PhoneNumber = Input.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, Input.Password);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}