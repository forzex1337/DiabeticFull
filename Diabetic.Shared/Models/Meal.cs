using System.ComponentModel.DataAnnotations;

namespace Diabetic.Shared.Models;

public class Meal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    [Required]
    public string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, Snack
    
    public string? Name { get; set; }
    public DateTime MealTime { get; set; }
    public string? Notes { get; set; }
    public string? PhotoUrl { get; set; }
    
    // Calculated totals
    public double TotalCalories { get; set; }
    public double TotalCarbs { get; set; }
    public double TotalSugars { get; set; }
    public double TotalFiber { get; set; }
    public double TotalProtein { get; set; }
    public double TotalFat { get; set; }
    public double EstimatedInsulinUnits { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = default!;
    public List<MealItem> MealItems { get; set; } = new();
    public List<GlucoseReading> GlucoseReadings { get; set; } = new();
    public List<InsulinRecord> InsulinRecords { get; set; } = new();
}