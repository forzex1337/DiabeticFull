using Microsoft.EntityFrameworkCore;
using Diabetic.Shared.Models;

namespace Diabetic.Data;

public class DiabeticDbContext : DbContext
{
    public DiabeticDbContext(DbContextOptions<DiabeticDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<GlucoseReading> GlucoseReadings { get; set; }
    public DbSet<InsulinRecord> InsulinRecords { get; set; }
    public DbSet<FoodProduct> FoodProducts { get; set; }
    public DbSet<Meal> Meals { get; set; }
    public DbSet<MealItem> MealItems { get; set; }
    public DbSet<Medication> Medications { get; set; }
    public DbSet<MedicationReminder> MedicationReminders { get; set; }
    public DbSet<DailyNote> DailyNotes { get; set; }
    public DbSet<Report> Reports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.DiabetesType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.EmergencyContact).HasMaxLength(100);
            entity.Property(e => e.EmergencyPhone).HasMaxLength(20);
            entity.Property(e => e.Doctor).HasMaxLength(100);
            entity.Property(e => e.DoctorPhone).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // GlucoseReading entity configuration
        modelBuilder.Entity<GlucoseReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).IsRequired().HasColumnType("decimal(5,2)");
            entity.Property(e => e.MeasurementType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Mood).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasOne(e => e.User)
                .WithMany(u => u.GlucoseReadings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Meal)
                .WithMany(m => m.GlucoseReadings)
                .HasForeignKey(e => e.MealId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // InsulinRecord entity configuration
        modelBuilder.Entity<InsulinRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.InsulinType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Dose).IsRequired().HasColumnType("decimal(4,2)");
            entity.Property(e => e.InjectionSite).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasOne(e => e.User)
                .WithMany(u => u.InsulinRecords)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Meal)
                .WithMany(m => m.InsulinRecords)
                .HasForeignKey(e => e.MealId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // FoodProduct entity configuration
        modelBuilder.Entity<FoodProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.CaloriesPer100g).HasColumnType("decimal(7,2)");
            entity.Property(e => e.CarbsPer100g).HasColumnType("decimal(5,2)");
            entity.Property(e => e.SugarsPer100g).HasColumnType("decimal(5,2)");
            entity.Property(e => e.FiberPer100g).HasColumnType("decimal(5,2)");
            entity.Property(e => e.ProteinPer100g).HasColumnType("decimal(5,2)");
            entity.Property(e => e.FatPer100g).HasColumnType("decimal(5,2)");
            entity.Property(e => e.SodiumPer100g).HasColumnType("decimal(7,2)");
            entity.Property(e => e.Source).IsRequired().HasMaxLength(50);
            entity.Property(e => e.OpenFoodFactsId).HasMaxLength(100);
            entity.HasIndex(e => e.Barcode);
            entity.HasIndex(e => e.OpenFoodFactsId);
        });

        // Meal entity configuration
        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MealType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PhotoUrl).HasMaxLength(500);
            entity.Property(e => e.TotalCalories).HasColumnType("decimal(7,2)");
            entity.Property(e => e.TotalCarbs).HasColumnType("decimal(6,2)");
            entity.Property(e => e.TotalSugars).HasColumnType("decimal(6,2)");
            entity.Property(e => e.TotalFiber).HasColumnType("decimal(6,2)");
            entity.Property(e => e.TotalProtein).HasColumnType("decimal(6,2)");
            entity.Property(e => e.TotalFat).HasColumnType("decimal(6,2)");
            entity.Property(e => e.EstimatedInsulinUnits).HasColumnType("decimal(4,2)");
            entity.HasOne(e => e.User)
                .WithMany(u => u.Meals)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MealItem entity configuration
        modelBuilder.Entity<MealItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).IsRequired().HasColumnType("decimal(8,2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Calories).HasColumnType("decimal(7,2)");
            entity.Property(e => e.Carbs).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Sugars).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Fiber).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Protein).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Fat).HasColumnType("decimal(6,2)");
            entity.Property(e => e.Sodium).HasColumnType("decimal(7,2)");
            entity.HasOne(e => e.Meal)
                .WithMany(m => m.MealItems)
                .HasForeignKey(e => e.MealId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.FoodProduct)
                .WithMany(fp => fp.MealItems)
                .HasForeignKey(e => e.FoodProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Medication entity configuration
        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.MedicationType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Dosage).HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(500);
            entity.Property(e => e.Frequency).HasMaxLength(100);
            entity.Property(e => e.PrescribedBy).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Medications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MedicationReminder entity configuration
        modelBuilder.Entity<MedicationReminder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasOne(e => e.Medication)
                .WithMany(m => m.MedicationReminders)
                .HasForeignKey(e => e.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DailyNote entity configuration
        modelBuilder.Entity<DailyNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Mood).HasMaxLength(50);
            entity.Property(e => e.PhysicalActivity).HasMaxLength(200);
            entity.Property(e => e.Weight).HasColumnType("decimal(5,2)");
            entity.Property(e => e.HoursOfSleep).HasColumnType("decimal(3,1)");
            entity.Property(e => e.Symptoms).HasMaxLength(500);
            entity.HasOne(e => e.User)
                .WithMany(u => u.DailyNotes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique();
        });

        // Report entity configuration
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Summary).HasMaxLength(2000);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.ShareToken).HasMaxLength(100);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.ShareToken).IsUnique();
        });
    }
}