using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Diabetic.Data;
using Diabetic.Shared.Models;

namespace Diabetic.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserDataController : ControllerBase
{
    private readonly DiabeticDbContext _context;
    private readonly UserManager<DiabeticUser> _userManager;

    public UserDataController(DiabeticDbContext context, UserManager<DiabeticUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    #region InsulinRecords

    [HttpGet("insulin-records")]
    public async Task<ActionResult<List<InsulinRecord>>> GetMyInsulinRecords(DateTime? startDate = null, DateTime? endDate = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _context.InsulinRecords
            .Where(i => i.UserId == user.Id);

        if (startDate.HasValue)
            query = query.Where(i => i.InjectionTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.InjectionTime <= endDate.Value);

        var records = await query
            .OrderByDescending(i => i.InjectionTime)
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("insulin-records/{id}")]
    public async Task<ActionResult<InsulinRecord>> GetInsulinRecord(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var record = await _context.InsulinRecords
            .Include(i => i.Meal)
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == user.Id);

        if (record == null)
            return NotFound();

        return Ok(record);
    }

    [HttpPost("insulin-records")]
    public async Task<ActionResult<InsulinRecord>> AddInsulinRecord(InsulinRecord record)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        record.UserId = user.Id;
        record.CreatedAt = DateTime.UtcNow;

        _context.InsulinRecords.Add(record);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInsulinRecord), new { id = record.Id }, record);
    }

    [HttpPut("insulin-records/{id}")]
    public async Task<ActionResult<InsulinRecord>> UpdateInsulinRecord(int id, InsulinRecord updatedRecord)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var existing = await _context.InsulinRecords
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == user.Id);

        if (existing == null)
            return NotFound();

        existing.InsulinType = updatedRecord.InsulinType;
        existing.Dose = updatedRecord.Dose;
        existing.InjectionTime = updatedRecord.InjectionTime;
        existing.InjectionSite = updatedRecord.InjectionSite;
        existing.Notes = updatedRecord.Notes;
        existing.MealId = updatedRecord.MealId;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("insulin-records/{id}")]
    public async Task<ActionResult> DeleteInsulinRecord(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var record = await _context.InsulinRecords
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == user.Id);

        if (record == null)
            return NotFound();

        _context.InsulinRecords.Remove(record);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion

    #region Medications

    [HttpGet("medications")]
    public async Task<ActionResult<List<Medication>>> GetMyMedications()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var medications = await _context.Medications
            .Where(m => m.UserId == user.Id)
            .OrderBy(m => m.Name)
            .ToListAsync();

        return Ok(medications);
    }

    [HttpGet("medications/{id}")]
    public async Task<ActionResult<Medication>> GetMedication(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var medication = await _context.Medications
            .Include(m => m.MedicationReminders)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

        if (medication == null)
            return NotFound();

        return Ok(medication);
    }

    [HttpPost("medications")]
    public async Task<ActionResult<Medication>> AddMedication(Medication medication)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        medication.UserId = user.Id;
        medication.CreatedAt = DateTime.UtcNow;
        medication.UpdatedAt = DateTime.UtcNow;

        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMedication), new { id = medication.Id }, medication);
    }

    [HttpPut("medications/{id}")]
    public async Task<ActionResult<Medication>> UpdateMedication(int id, Medication updatedMedication)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var existing = await _context.Medications
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

        if (existing == null)
            return NotFound();

        existing.Name = updatedMedication.Name;
        existing.Brand = updatedMedication.Brand;
        existing.MedicationType = updatedMedication.MedicationType;
        existing.Dosage = updatedMedication.Dosage;
        existing.Instructions = updatedMedication.Instructions;
        existing.Frequency = updatedMedication.Frequency;
        existing.PrescribedBy = updatedMedication.PrescribedBy;
        existing.StartDate = updatedMedication.StartDate;
        existing.EndDate = updatedMedication.EndDate;
        existing.IsActive = updatedMedication.IsActive;
        existing.Notes = updatedMedication.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("medications/{id}")]
    public async Task<ActionResult> DeleteMedication(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

        if (medication == null)
            return NotFound();

        _context.Medications.Remove(medication);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion

    #region DailyNotes

    [HttpGet("daily-notes")]
    public async Task<ActionResult<List<DailyNote>>> GetMyDailyNotes(DateTime? startDate = null, DateTime? endDate = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var query = _context.DailyNotes
            .Where(d => d.UserId == user.Id);

        if (startDate.HasValue)
            query = query.Where(d => d.Date >= DateOnly.FromDateTime(startDate.Value));

        if (endDate.HasValue)
            query = query.Where(d => d.Date <= DateOnly.FromDateTime(endDate.Value));

        var notes = await query
            .OrderByDescending(d => d.Date)
            .ToListAsync();

        return Ok(notes);
    }

    [HttpGet("daily-notes/{id}")]
    public async Task<ActionResult<DailyNote>> GetDailyNote(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var note = await _context.DailyNotes
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == user.Id);

        if (note == null)
            return NotFound();

        return Ok(note);
    }

    [HttpGet("daily-notes/date/{date}")]
    public async Task<ActionResult<DailyNote>> GetDailyNoteByDate(DateTime date)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var dateOnly = DateOnly.FromDateTime(date);
        var note = await _context.DailyNotes
            .FirstOrDefaultAsync(d => d.Date == dateOnly && d.UserId == user.Id);

        if (note == null)
        {
            return Ok(new DailyNote
            {
                UserId = user.Id,
                Date = dateOnly,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        return Ok(note);
    }

    [HttpPost("daily-notes")]
    public async Task<ActionResult<DailyNote>> AddDailyNote(DailyNote note)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        note.UserId = user.Id;
        note.CreatedAt = DateTime.UtcNow;
        note.UpdatedAt = DateTime.UtcNow;

        _context.DailyNotes.Add(note);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDailyNote), new { id = note.Id }, note);
    }

    [HttpPut("daily-notes/{id}")]
    public async Task<ActionResult<DailyNote>> UpdateDailyNote(int id, DailyNote updatedNote)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var existing = await _context.DailyNotes
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == user.Id);

        if (existing == null)
            return NotFound();

        existing.Notes = updatedNote.Notes;
        existing.Mood = updatedNote.Mood;
        existing.PhysicalActivity = updatedNote.PhysicalActivity;
        existing.Weight = updatedNote.Weight;
        existing.HoursOfSleep = updatedNote.HoursOfSleep;
        existing.Symptoms = updatedNote.Symptoms;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("daily-notes/{id}")]
    public async Task<ActionResult> DeleteDailyNote(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var note = await _context.DailyNotes
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == user.Id);

        if (note == null)
            return NotFound();

        _context.DailyNotes.Remove(note);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    #endregion

    #region Reports

    [HttpGet("reports")]
    public async Task<ActionResult<List<Report>>> GetMyReports()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var reports = await _context.Reports
            .Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(reports);
    }

    [HttpPost("reports")]
    public async Task<ActionResult<Report>> CreateReport(Report report)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        report.UserId = user.Id;
        report.CreatedAt = DateTime.UtcNow;
        report.ShareToken = Guid.NewGuid().ToString("N");

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return Ok(report);
    }

    #endregion
}