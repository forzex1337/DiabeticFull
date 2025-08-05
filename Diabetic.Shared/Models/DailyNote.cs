namespace Diabetic.Shared.Models;

public class DailyNote
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
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
    public User User { get; set; } = default!;
}