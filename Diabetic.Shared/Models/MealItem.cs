using System.ComponentModel.DataAnnotations;

namespace Diabetic.Shared.Models;

public class MealItem
{
    public int Id { get; set; }
    public int MealId { get; set; }
    public int FoodProductId { get; set; }
    
    [Range(0.1, 10000)]
    public double Quantity { get; set; } // in grams
    
    public string? Notes { get; set; }
    
    // Calculated nutritional values for this portion
    public double Calories { get; set; }
    public double Carbs { get; set; }
    public double Sugars { get; set; }
    public double Fiber { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Sodium { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Meal Meal { get; set; } = default!;
    public FoodProduct FoodProduct { get; set; } = default!;
}