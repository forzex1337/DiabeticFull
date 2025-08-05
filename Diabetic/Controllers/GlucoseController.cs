using Microsoft.AspNetCore.Mvc;
using Diabetic.Shared.Models;
using Diabetic.Services;

namespace Diabetic.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GlucoseController : ControllerBase
{
    private readonly GlucoseService _glucoseService;

    public GlucoseController(GlucoseService glucoseService)
    {
        _glucoseService = glucoseService;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<GlucoseReading>>> GetReadingsByUser(int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var readings = await _glucoseService.GetReadingsByUserAsync(userId, startDate, endDate);
        return Ok(readings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GlucoseReading>> GetReading(int id)
    {
        var reading = await _glucoseService.GetReadingByIdAsync(id);
        if (reading == null)
            return NotFound();

        return Ok(reading);
    }

    [HttpPost]
    public async Task<ActionResult<GlucoseReading>> AddReading(GlucoseReading reading)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _glucoseService.AddReadingAsync(reading);
        return CreatedAtAction(nameof(GetReading), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<GlucoseReading>> UpdateReading(int id, GlucoseReading reading)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _glucoseService.UpdateReadingAsync(id, reading);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReading(int id)
    {
        var success = await _glucoseService.DeleteReadingAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpGet("user/{userId}/statistics")]
    public async Task<ActionResult<GlucoseStatistics>> GetStatistics(int userId, DateTime startDate, DateTime endDate)
    {
        var statistics = await _glucoseService.GetStatisticsAsync(userId, startDate, endDate);
        return Ok(statistics);
    }
}