using System.ComponentModel.DataAnnotations;

namespace Diabetic.Shared.Models;

public class GlucoseReading
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    [Range(1, 1000)]
    public double Value { get; set; } // mg/dL
    
    public DateTime MeasurementTime { get; set; }
    
    [Required]
    public string MeasurementType { get; set; } = string.Empty; // Fasting, PreMeal, PostMeal, Bedtime, Random
    
    public string? Mood { get; set; }
    public string? Notes { get; set; }
    public int? MealId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public DiabeticUser User { get; set; } = default!;
    public Meal? Meal { get; set; }
}