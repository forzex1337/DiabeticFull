using System.ComponentModel.DataAnnotations;

namespace Diabetic.Shared.Models;

public class FoodProduct
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Brand { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    // Nutritional info per 100g
    [Range(0, 10000)]
    public double CaloriesPer100g { get; set; }

    [Range(0, 100)]
    public double CarbsPer100g { get; set; }

    [Range(0, 100)]
    public double SugarsPer100g { get; set; }

    [Range(0, 100)]
    public double FiberPer100g { get; set; }

    [Range(0, 100)]
    public double ProteinPer100g { get; set; }

    [Range(0, 100)]
    public double FatPer100g { get; set; }

    [Range(0, 10000)]
    public double SodiumPer100g { get; set; } // mg

    [Range(0, 100)]
    public int? GlycemicIndex { get; set; }

    public string Source { get; set; } = string.Empty; // OpenFoodFacts, Manual, etc.
    public string? OpenFoodFactsId { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties for EF Core
    public List<MealItem> MealItems { get; set; } = new();
}