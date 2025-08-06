using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Diabetic.Data;
using Diabetic.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Entity Framework
builder.Services.AddDbContext<DiabeticDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        new MySqlServerVersion(new Version(8, 0, 21))));

// Add Identity
builder.Services.AddIdentity<DiabeticUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<DiabeticDbContext>()
.AddDefaultTokenProviders();

// Configure claims transformation
builder.Services.AddScoped<IUserClaimsPrincipalFactory<DiabeticUser>, DiabeticUserClaimsPrincipalFactory>();

// Add OAuth authentication
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    })
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? "";
        facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? "";
    });

// Add HTTP client configured for the API
builder.Services.AddHttpClient("DiabeticAPI", client =>
{
    var baseUrl = builder.Configuration["DiabeticApi:BaseUrl"] ?? "https://localhost:7030/";
    client.BaseAddress = new Uri(baseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    handler.CookieContainer = new System.Net.CookieContainer();
    handler.UseCookies = true;
    return handler;
});

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DiabeticAPI"));

// Add API configuration
builder.Services.Configure<Diabetic.Shared.Services.ApiConfiguration>(options =>
{
    options.BaseUrl = builder.Configuration["DiabeticApi:BaseUrl"] ?? "https://localhost:7030/";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Add authentication endpoints
app.MapPost("/api/auth/login", async (LoginRequest request, UserManager<DiabeticUser> userManager, 
    SignInManager<DiabeticUser> signInManager, ILogger<Program> logger) =>
{
    try
    {
        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, request.RememberMe, lockoutOnFailure: false);
        
        if (result.Succeeded)
        {
            logger.LogInformation("User logged in successfully.");
            return Results.Ok(new { success = true, message = "Login successful" });
        }
        else if (result.IsLockedOut)
        {
            return Results.BadRequest(new { success = false, message = "Konto zostało zablokowane." });
        }
        else
        {
            return Results.BadRequest(new { success = false, message = "Nieprawidłowy email lub hasło." });
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Login error");
        return Results.Problem("Wystąpił błąd podczas logowania.");
    }
});

app.MapPost("/api/auth/register", async (RegisterRequest request, UserManager<DiabeticUser> userManager, 
    SignInManager<DiabeticUser> signInManager, ILogger<Program> logger) =>
{
    try
    {
        var user = new DiabeticUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DiabetesType = request.DiabetesType,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Height = request.Height,
            Weight = request.Weight,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            logger.LogInformation("User created a new account.");

            // Add user to BasicUser role
            try
            {
                await userManager.AddToRoleAsync(user, "BasicUser");
                logger.LogInformation("User added to BasicUser role.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to add user to BasicUser role.");
            }

            // Sign in the user
            await signInManager.SignInAsync(user, isPersistent: false);
            return Results.Ok(new { success = true, message = "Registration successful" });
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            return Results.BadRequest(new { success = false, message = errors });
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Registration error");
        return Results.Problem("Wystąpił błąd podczas rejestracji.");
    }
});

app.MapPost("/api/auth/logout", async (SignInManager<DiabeticUser> signInManager, ILogger<Program> logger) =>
{
    try
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out successfully.");
        return Results.Ok(new { success = true, message = "Logout successful" });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Logout error");
        return Results.Problem("Wystąpił błąd podczas wylogowania.");
    }
});


app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

// Ensure database exists and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DiabeticDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Checking database connection...");
        
        // Try to connect to the database, if it fails try to create the database
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogInformation("Cannot connect to database. Attempting to create database...");
            try
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created successfully.");
            }
            catch (Exception createEx)
            {
                logger.LogError(createEx, "Failed to create database. Please check your connection string and permissions.");
                return;
            }
        }
        else
        {
            logger.LogInformation("Database connection successful.");
            
            // Apply migrations to create database schema  
            try
            {
                logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
            catch (Exception migrationEx)
            {
                logger.LogWarning(migrationEx, "Migration failed. Trying to ensure database is created...");
                
                try
                {
                    var created = await context.Database.EnsureCreatedAsync();
                    if (created)
                    {
                        logger.LogInformation("Database schema created with EnsureCreated.");
                    }
                    else
                    {
                        logger.LogInformation("Database schema already exists.");
                    }
                }
                catch (Exception ensureEx)
                {
                    logger.LogError(ensureEx, "Both migrations and EnsureCreated failed. Database may not work properly.");
                }
            }
            
            // Create roles if they don't exist
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = { "Admin", "BasicUser", "PremiumUser", "ProUser" };
            
            foreach (string role in roles)
            {
                try
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                        logger.LogInformation($"Role '{role}' created successfully.");
                    }
                }
                catch (Exception roleEx)
                {
                    logger.LogWarning(roleEx, $"Failed to create role '{role}'. Continuing...");
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while checking database. Application will continue to start.");
        // Don't throw - let the app start even if database check fails
    }
}

app.Run();

// Request models for authentication endpoints
public record LoginRequest(string Email, string Password, bool RememberMe);

public record RegisterRequest(
    string Email, 
    string Password, 
    string? FirstName, 
    string? LastName, 
    string DiabetesType, 
    DateTime? DateOfBirth, 
    string? Gender, 
    double? Height, 
    double? Weight
);

// Custom claims principal factory to add user-specific claims
public class DiabeticUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<DiabeticUser, IdentityRole>
{
    public DiabeticUserClaimsPrincipalFactory(UserManager<DiabeticUser> userManager, 
        RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(DiabeticUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        
        // Add custom claims
        if (!string.IsNullOrWhiteSpace(user.FirstName))
        {
            identity.AddClaim(new Claim("FirstName", user.FirstName));
        }
        
        if (!string.IsNullOrWhiteSpace(user.LastName))
        {
            identity.AddClaim(new Claim("LastName", user.LastName));
        }
        
        return identity;
    }
}
