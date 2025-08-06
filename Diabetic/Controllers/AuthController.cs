using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Diabetic.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Diabetic.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<DiabeticUser> _userManager;
    private readonly SignInManager<DiabeticUser> _signInManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<DiabeticUser> userManager,
        SignInManager<DiabeticUser> signInManager,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new DiabeticUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            DiabetesType = model.DiabetesType,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            Height = model.Height,
            Weight = model.Weight,
            PhoneNumber = model.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            _logger.LogInformation("User created a new account with password.");
            await _signInManager.SignInAsync(user, isPersistent: false);
            
            return Ok(new { 
                message = "Registration successful", 
                userId = user.Id, 
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName
            });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            _logger.LogInformation("User logged in.");
            
            return Ok(new { 
                message = "Login successful", 
                userId = user?.Id, 
                email = user?.Email,
                firstName = user?.FirstName,
                lastName = user?.LastName
            });
        }

        if (result.RequiresTwoFactor)
        {
            return BadRequest("Two-factor authentication required.");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return BadRequest("Account locked out.");
        }

        return BadRequest("Invalid login attempt.");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            diabetesType = user.DiabetesType,
            dateOfBirth = user.DateOfBirth,
            gender = user.Gender,
            height = user.Height,
            weight = user.Weight,
            phoneNumber = user.PhoneNumber,
            emergencyContact = user.EmergencyContact,
            emergencyPhone = user.EmergencyPhone,
            doctor = user.Doctor,
            doctorPhone = user.DoctorPhone,
            notes = user.Notes
        });
    }

    [HttpPut("user")]
    [Authorize]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.DiabetesType = model.DiabetesType;
        user.DateOfBirth = model.DateOfBirth;
        user.Gender = model.Gender;
        user.Height = model.Height;
        user.Weight = model.Weight;
        user.PhoneNumber = model.PhoneNumber;
        user.EmergencyContact = model.EmergencyContact;
        user.EmergencyPhone = model.EmergencyPhone;
        user.Doctor = model.Doctor;
        user.DoctorPhone = model.DoctorPhone;
        user.Notes = model.Notes;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return Ok(new { message = "User updated successfully" });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpPost("external-login/{provider}")]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet("external-login-callback")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        
        if (remoteError != null)
        {
            return BadRequest($"Error from external provider: {remoteError}");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return BadRequest("Error loading external login information.");
        }

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        
        if (result.Succeeded)
        {
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);
            
            return Ok(new { 
                message = "External login successful", 
                userId = user?.Id, 
                email = user?.Email,
                firstName = user?.FirstName,
                lastName = user?.LastName,
                provider = info.LoginProvider
            });
        }
        
        if (result.IsLockedOut)
        {
            return BadRequest("Account locked out.");
        }
        else
        {
            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var firstName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value;
            var lastName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value;

            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new DiabeticUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        DiabetesType = "Unknown",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    await _userManager.CreateAsync(user);
                }

                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                return Ok(new { 
                    message = "External login successful", 
                    userId = user.Id, 
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    provider = info.LoginProvider,
                    isNewUser = true
                });
            }

            return BadRequest("Unable to load user information from external provider.");
        }
    }
}

public class RegisterModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    [Required]
    public string DiabetesType { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public string? PhoneNumber { get; set; }
}

public class LoginModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class UpdateUserModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    [Required]
    public string DiabetesType { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? Doctor { get; set; }
    public string? DoctorPhone { get; set; }
    public string? Notes { get; set; }
}