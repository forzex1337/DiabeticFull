using Microsoft.EntityFrameworkCore;
using Diabetic.Data;
using Diabetic.Shared.Models;

namespace Diabetic.Services;

public class GlucoseService
{
    private readonly DiabeticDbContext _context;

    public GlucoseService(DiabeticDbContext context)
    {
        _context = context;
    }

    public async Task<List<GlucoseReading>> GetReadingsByUserAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.GlucoseReadings
            .Where(g => g.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(g => g.MeasurementTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(g => g.MeasurementTime <= endDate.Value);

        return await query
            .OrderByDescending(g => g.MeasurementTime)
            .ToListAsync();
    }

    public async Task<GlucoseReading?> GetReadingByIdAsync(int id)
    {
        return await _context.GlucoseReadings
            .Include(g => g.Meal)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GlucoseReading> AddReadingAsync(GlucoseReading reading)
    {
        reading.CreatedAt = DateTime.UtcNow;
        _context.GlucoseReadings.Add(reading);
        await _context.SaveChangesAsync();
        return reading;
    }

    public async Task<GlucoseReading?> UpdateReadingAsync(int id, GlucoseReading updatedReading)
    {
        var existing = await _context.GlucoseReadings.FindAsync(id);
        if (existing == null)
            return null;

        existing.Value = updatedReading.Value;
        existing.MeasurementTime = updatedReading.MeasurementTime;
        existing.MeasurementType = updatedReading.MeasurementType;
        existing.Notes = updatedReading.Notes;
        existing.Mood = updatedReading.Mood;
        existing.MealId = updatedReading.MealId;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteReadingAsync(int id)
    {
        var reading = await _context.GlucoseReadings.FindAsync(id);
        if (reading == null)
            return false;

        _context.GlucoseReadings.Remove(reading);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<GlucoseStatistics> GetStatisticsAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var readings = await _context.GlucoseReadings
            .Where(g => g.UserId == userId && g.MeasurementTime >= startDate && g.MeasurementTime <= endDate)
            .ToListAsync();

        if (!readings.Any())
        {
            return new GlucoseStatistics
            {
                AverageGlucose = 0,
                MinGlucose = 0,
                MaxGlucose = 0,
                ReadingCount = 0,
                InRangePercentage = 0,
                BelowRangePercentage = 0,
                AboveRangePercentage = 0
            };
        }

        var values = readings.Select(r => r.Value).ToList();
        var average = values.Average();
        var min = values.Min();
        var max = values.Max();

        // Typical target ranges (can be customized per user)
        const double targetMin = 80;  // mg/dL
        const double targetMax = 180; // mg/dL

        var inRange = readings.Count(r => r.Value >= targetMin && r.Value <= targetMax);
        var belowRange = readings.Count(r => r.Value < targetMin);
        var aboveRange = readings.Count(r => r.Value > targetMax);

        return new GlucoseStatistics
        {
            AverageGlucose = average,
            MinGlucose = min,
            MaxGlucose = max,
            ReadingCount = readings.Count,
            InRangePercentage = (double)inRange / readings.Count * 100,
            BelowRangePercentage = (double)belowRange / readings.Count * 100,
            AboveRangePercentage = (double)aboveRange / readings.Count * 100
        };
    }
}

public class GlucoseStatistics
{
    public double AverageGlucose { get; set; }
    public double MinGlucose { get; set; }
    public double MaxGlucose { get; set; }
    public int ReadingCount { get; set; }
    public double InRangePercentage { get; set; }
    public double BelowRangePercentage { get; set; }
    public double AboveRangePercentage { get; set; }
}