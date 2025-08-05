namespace Diabetic.Shared.Models;

public class MedicationReminder
{
    public int Id { get; set; }
    public int MedicationId { get; set; }
    
    public TimeOnly ReminderTime { get; set; }
    public DateTime ScheduledDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Medication Medication { get; set; } = default!;
}