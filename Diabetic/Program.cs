using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Diabetic.Data;
using Diabetic.Services;
using Diabetic.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("https://localhost:7058", "https://localhost:7195", "https://localhost:5001", "http://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DiabeticDbContext>()
.AddDefaultTokenProviders();

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

// Add HTTP client for OpenFoodFacts API
builder.Services.AddHttpClient<OpenFoodFactsService>(client =>
{
    client.BaseAddress = new Uri("https://world.openfoodfacts.org/api/v0/");
    client.DefaultRequestHeaders.Add("User-Agent", "DiabeticApp/1.0");
});

// Add custom services
builder.Services.AddScoped<OpenFoodFactsService>();
builder.Services.AddScoped<GlucoseService>();
builder.Services.AddScoped<MealService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
        
        logger.LogInformation("Database connection successful.");
        
        // Apply migrations to create database schema
        try
        {
            logger.LogInformation("Checking for pending migrations...");
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                logger.LogInformation($"Found {pendingMigrations.Count()} pending migrations. Applying...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is up to date.");
            }
        }
        catch (Exception migrationEx)
        {
            logger.LogError(migrationEx, "Migration failed.");
            
            // If migrations fail completely, try to recreate database
            try
            {
                logger.LogWarning("Attempting to delete and recreate database...");
                await context.Database.EnsureDeletedAsync();
                await context.Database.MigrateAsync();
                logger.LogInformation("Database recreated successfully with migrations.");
            }
            catch (Exception recreateEx)
            {
                logger.LogError(recreateEx, "Failed to recreate database. Manual intervention may be required.");
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
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while checking database. Application will continue to start.");
        // Don't throw - let the app start even if database check fails
    }
}

app.Run();