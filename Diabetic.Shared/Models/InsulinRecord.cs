using System.ComponentModel.DataAnnotations;

namespace Diabetic.Shared.Models;

public class InsulinRecord
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string InsulinType { get; set; } = string.Empty; // Rapid, Long, Mixed
    
    [Range(0.1, 100)]
    public double Dose { get; set; } // units
    
    public DateTime InjectionTime { get; set; }
    public string? InjectionSite { get; set; }
    public string? Notes { get; set; }
    public int? MealId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public DiabeticUser User { get; set; } = default!;
    public Meal? Meal { get; set; }
}