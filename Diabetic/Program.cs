using Microsoft.EntityFrameworkCore;
using Diabetic.Data;
using Diabetic.Services;

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
        policy.WithOrigins("https://localhost:7001", "http://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Entity Framework
builder.Services.AddDbContext<DiabeticDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
app.MapControllers();

app.Run();