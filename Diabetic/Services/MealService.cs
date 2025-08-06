using Microsoft.EntityFrameworkCore;
using Diabetic.Data;
using Diabetic.Shared.Models;

namespace Diabetic.Services;

public class MealService
{
    private readonly DiabeticDbContext _context;

    public MealService(DiabeticDbContext context)
    {
        _context = context;
    }

    public async Task<List<Meal>> GetMealsByUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Meals
            .Include(m => m.MealItems)
            .ThenInclude(mi => mi.FoodProduct)
            .Where(m => m.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(m => m.MealTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(m => m.MealTime <= endDate.Value);

        return await query
            .OrderByDescending(m => m.MealTime)
            .ToListAsync();
    }

    public async Task<Meal?> GetMealByIdAsync(int id)
    {
        return await _context.Meals
            .Include(m => m.MealItems)
            .ThenInclude(mi => mi.FoodProduct)
            .Include(m => m.GlucoseReadings)
            .Include(m => m.InsulinRecords)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Meal> AddMealAsync(Meal meal)
    {
        meal.CreatedAt = DateTime.UtcNow;
        meal.UpdatedAt = DateTime.UtcNow;
        
        // Calculate totals from meal items
        CalculateMealTotals(meal);
        
        _context.Meals.Add(meal);
        await _context.SaveChangesAsync();
        return meal;
    }

    public async Task<MealItem> AddMealItemAsync(int mealId, MealItem mealItem)
    {
        var meal = await _context.Meals.FindAsync(mealId);
        if (meal == null)
            throw new ArgumentException("Meal not found");

        var foodProduct = await _context.FoodProducts.FindAsync(mealItem.FoodProductId);
        if (foodProduct == null)
            throw new ArgumentException("Food product not found");

        // Calculate nutritional values based on quantity
        CalculateMealItemValues(mealItem, foodProduct);
        
        mealItem.MealId = mealId;
        mealItem.CreatedAt = DateTime.UtcNow;
        
        _context.MealItems.Add(mealItem);
        
        // Recalculate meal totals
        await RecalculateMealTotalsAsync(mealId);
        
        await _context.SaveChangesAsync();
        return mealItem;
    }

    public async Task<bool> RemoveMealItemAsync(int mealItemId)
    {
        var mealItem = await _context.MealItems.FindAsync(mealItemId);
        if (mealItem == null)
            return false;

        var mealId = mealItem.MealId;
        _context.MealItems.Remove(mealItem);
        
        // Recalculate meal totals
        await RecalculateMealTotalsAsync(mealId);
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Meal?> UpdateMealAsync(int id, Meal updatedMeal)
    {
        var existing = await _context.Meals.FindAsync(id);
        if (existing == null)
            return null;

        existing.MealType = updatedMeal.MealType;
        existing.MealTime = updatedMeal.MealTime;
        existing.Name = updatedMeal.Name;
        existing.Notes = updatedMeal.Notes;
        existing.PhotoUrl = updatedMeal.PhotoUrl;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteMealAsync(int id)
    {
        var meal = await _context.Meals
            .Include(m => m.MealItems)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (meal == null)
            return false;

        _context.Meals.Remove(meal);
        await _context.SaveChangesAsync();
        return true;
    }

    private static void CalculateMealItemValues(MealItem mealItem, FoodProduct foodProduct)
    {
        var factor = mealItem.Quantity / 100.0; // Convert to per-gram basis
        
        mealItem.Calories = foodProduct.CaloriesPer100g * factor;
        mealItem.Carbs = foodProduct.CarbsPer100g * factor;
        mealItem.Sugars = foodProduct.SugarsPer100g * factor;
        mealItem.Fiber = foodProduct.FiberPer100g * factor;
        mealItem.Protein = foodProduct.ProteinPer100g * factor;
        mealItem.Fat = foodProduct.FatPer100g * factor;
        mealItem.Sodium = foodProduct.SodiumPer100g * factor;
    }

    private static void CalculateMealTotals(Meal meal)
    {
        meal.TotalCalories = meal.MealItems.Sum(mi => mi.Calories);
        meal.TotalCarbs = meal.MealItems.Sum(mi => mi.Carbs);
        meal.TotalSugars = meal.MealItems.Sum(mi => mi.Sugars);
        meal.TotalFiber = meal.MealItems.Sum(mi => mi.Fiber);
        meal.TotalProtein = meal.MealItems.Sum(mi => mi.Protein);
        meal.TotalFat = meal.MealItems.Sum(mi => mi.Fat);
        
        // Simple insulin estimation (can be customized per user)
        // Typically 1 unit per 10-15g carbs
        meal.EstimatedInsulinUnits = meal.TotalCarbs / 12.0;
    }

    private async Task RecalculateMealTotalsAsync(int mealId)
    {
        var meal = await _context.Meals
            .Include(m => m.MealItems)
            .FirstOrDefaultAsync(m => m.Id == mealId);

        if (meal != null)
        {
            CalculateMealTotals(meal);
            meal.UpdatedAt = DateTime.UtcNow;
        }
    }
}