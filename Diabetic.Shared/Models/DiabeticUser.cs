using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Diabetic.Shared.Models;

public class DiabeticUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    [Required]
    public string DiabetesType { get; set; } = string.Empty; // Type1, Type2, Gestational
    
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public double? Height { get; set; } // cm
    public double? Weight { get; set; } // kg
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? Doctor { get; set; }
    public string? DoctorPhone { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public List<GlucoseReading> GlucoseReadings { get; set; } = new();
    public List<InsulinRecord> InsulinRecords { get; set; } = new();
    public List<Meal> Meals { get; set; } = new();
    public List<Medication> Medications { get; set; } = new();
    public List<DailyNote> DailyNotes { get; set; } = new();
}