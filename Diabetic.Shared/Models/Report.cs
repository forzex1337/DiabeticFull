using System.ComponentModel.DataAnnotations;

namespace Diabetic.Shared.Models;

public class Report
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string ReportType { get; set; } = string.Empty; // Glucose, Insulin, Meal, Overall
    
    public string? Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Summary { get; set; }
    public string? FilePath { get; set; }
    public string? ShareToken { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public DiabeticUser User { get; set; } = default!;
}