using System.ComponentModel.DataAnnotations;

namespace Diabetic.Shared.Models;

public class Medication
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Brand { get; set; }
    
    [Required]
    public string MedicationType { get; set; } = string.Empty; // Insulin, Oral, Injectable
    
    public string? Dosage { get; set; }
    public string? Instructions { get; set; }
    public string? Frequency { get; set; }
    public string? PrescribedBy { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = default!;
    public List<MedicationReminder> MedicationReminders { get; set; } = new();
}