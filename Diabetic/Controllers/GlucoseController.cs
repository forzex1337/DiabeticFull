using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Diabetic.Shared.Models;
using Diabetic.Services;

namespace Diabetic.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GlucoseController : ControllerBase
{
    private readonly GlucoseService _glucoseService;
    private readonly UserManager<DiabeticUser> _userManager;

    public GlucoseController(GlucoseService glucoseService, UserManager<DiabeticUser> userManager)
    {
        _glucoseService = glucoseService;
        _userManager = userManager;
    }

    [HttpGet("my-readings")]
    public async Task<ActionResult<List<GlucoseReading>>> GetMyReadings(DateTime? startDate = null, DateTime? endDate = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        
        var readings = await _glucoseService.GetReadingsByUserAsync(user.Id, startDate, endDate);
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

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        
        reading.UserId = user.Id;
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

    [HttpGet("my-statistics")]
    public async Task<ActionResult<GlucoseStatistics>> GetMyStatistics(DateTime startDate, DateTime endDate)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        
        var statistics = await _glucoseService.GetStatisticsAsync(user.Id, startDate, endDate);
        return Ok(statistics);
    }
}