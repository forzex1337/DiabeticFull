using System.ComponentModel.DataAnnotations;

namespace Diabetic.Shared.Models;

public class InsulinRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
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
    public User User { get; set; } = default!;
    public Meal? Meal { get; set; }
}