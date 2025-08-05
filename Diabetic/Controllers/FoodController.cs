using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Diabetic.Data;
using Diabetic.Shared.Models;
using Diabetic.Services;

namespace Diabetic.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FoodController : ControllerBase
{
    private readonly DiabeticDbContext _context;
    private readonly OpenFoodFactsService _openFoodFactsService;

    public FoodController(DiabeticDbContext context, OpenFoodFactsService openFoodFactsService)
    {
        _context = context;
        _openFoodFactsService = openFoodFactsService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<FoodProduct>>> SearchFood(string query, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");

        // First search local database
        var localResults = await _context.FoodProducts
            .Where(fp => fp.Name.Contains(query) || (fp.Brand != null && fp.Brand.Contains(query)))
            .Take(pageSize)
            .ToListAsync();

        // If we don't have enough local results, search OpenFoodFacts
        if (localResults.Count < pageSize)
        {
            var openFoodFactsResults = await _openFoodFactsService.SearchProductsAsync(query, pageSize - localResults.Count);
            
            // Save new products to local database
            foreach (var product in openFoodFactsResults)
            {
                var existing = await _context.FoodProducts.FirstOrDefaultAsync(fp => fp.Barcode == product.Barcode);
                if (existing == null)
                {
                    _context.FoodProducts.Add(product);
                }
            }
            await _context.SaveChangesAsync();

            localResults.AddRange(openFoodFactsResults);
        }

        return Ok(localResults);
    }

    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<FoodProduct>> GetByBarcode(string barcode)
    {
        // First check local database
        var localProduct = await _context.FoodProducts.FirstOrDefaultAsync(fp => fp.Barcode == barcode);
        if (localProduct != null)
            return Ok(localProduct);

        // If not found locally, search OpenFoodFacts
        var openFoodFactsProduct = await _openFoodFactsService.GetProductByBarcodeAsync(barcode);
        if (openFoodFactsProduct == null)
            return NotFound();

        // Save to local database
        _context.FoodProducts.Add(openFoodFactsProduct);
        await _context.SaveChangesAsync();

        return Ok(openFoodFactsProduct);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FoodProduct>> GetFood(int id)
    {
        var food = await _context.FoodProducts.FindAsync(id);
        if (food == null)
            return NotFound();

        return Ok(food);
    }

    [HttpPost]
    public async Task<ActionResult<FoodProduct>> AddCustomFood(FoodProduct foodProduct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        foodProduct.Source = "Manual";
        foodProduct.IsVerified = false;
        foodProduct.CreatedAt = DateTime.UtcNow;
        foodProduct.UpdatedAt = DateTime.UtcNow;

        _context.FoodProducts.Add(foodProduct);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFood), new { id = foodProduct.Id }, foodProduct);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FoodProduct>> UpdateFood(int id, FoodProduct foodProduct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _context.FoodProducts.FindAsync(id);
        if (existing == null)
            return NotFound();

        existing.Name = foodProduct.Name;
        existing.Brand = foodProduct.Brand;
        existing.Description = foodProduct.Description;
        existing.ImageUrl = foodProduct.ImageUrl;
        existing.CaloriesPer100g = foodProduct.CaloriesPer100g;
        existing.CarbsPer100g = foodProduct.CarbsPer100g;
        existing.SugarsPer100g = foodProduct.SugarsPer100g;
        existing.FiberPer100g = foodProduct.FiberPer100g;
        existing.ProteinPer100g = foodProduct.ProteinPer100g;
        existing.FatPer100g = foodProduct.FatPer100g;
        existing.SodiumPer100g = foodProduct.SodiumPer100g;
        existing.GlycemicIndex = foodProduct.GlycemicIndex;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFood(int id)
    {
        var food = await _context.FoodProducts.FindAsync(id);
        if (food == null)
            return NotFound();

        // Check if food is used in any meals
        var isUsed = await _context.MealItems.AnyAsync(mi => mi.FoodProductId == id);
        if (isUsed)
            return BadRequest("Cannot delete food product that is used in meals");

        _context.FoodProducts.Remove(food);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}