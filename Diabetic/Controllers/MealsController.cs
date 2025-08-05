using Microsoft.AspNetCore.Mvc;
using Diabetic.Shared.Models;
using Diabetic.Services;

namespace Diabetic.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MealsController : ControllerBase
{
    private readonly MealService _mealService;

    public MealsController(MealService mealService)
    {
        _mealService = mealService;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<Meal>>> GetMealsByUser(int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var meals = await _mealService.GetMealsByUserAsync(userId, startDate, endDate);
        return Ok(meals);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Meal>> GetMeal(int id)
    {
        var meal = await _mealService.GetMealByIdAsync(id);
        if (meal == null)
            return NotFound();

        return Ok(meal);
    }

    [HttpPost]
    public async Task<ActionResult<Meal>> AddMeal(Meal meal)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mealService.AddMealAsync(meal);
        return CreatedAtAction(nameof(GetMeal), new { id = result.Id }, result);
    }

    [HttpPost("{mealId}/items")]
    public async Task<ActionResult<MealItem>> AddMealItem(int mealId, MealItem mealItem)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _mealService.AddMealItemAsync(mealId, mealItem);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("items/{mealItemId}")]
    public async Task<ActionResult> RemoveMealItem(int mealItemId)
    {
        var success = await _mealService.RemoveMealItemAsync(mealItemId);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Meal>> UpdateMeal(int id, Meal meal)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _mealService.UpdateMealAsync(id, meal);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMeal(int id)
    {
        var success = await _mealService.DeleteMealAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}