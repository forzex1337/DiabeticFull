namespace Diabetic.Shared.Models;

public class DailyNote
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    public string? Mood { get; set; }
    public string? PhysicalActivity { get; set; }
    public double? Weight { get; set; }
    public double? HoursOfSleep { get; set; }
    public string? Symptoms { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public DiabeticUser User { get; set; } = default!;
}