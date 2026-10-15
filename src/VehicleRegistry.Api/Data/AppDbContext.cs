using Microsoft.EntityFrameworkCore;
using VehicleRegistry.Core.Domain;

namespace VehicleRegistry.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<WeightCategory> WeightCategories => Set<WeightCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Manufacturer>(e =>
        {
            e.Property(m => m.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(m => m.Name).IsUnique();

            e.HasData(
                new Manufacturer { Id = 1, Name = "Mazda" },
                new Manufacturer { Id = 2, Name = "Mercedes" },
                new Manufacturer { Id = 3, Name = "Honda" },
                new Manufacturer { Id = 4, Name = "Ferrari" },
                new Manufacturer { Id = 5, Name = "Toyota" });
        });

        modelBuilder.Entity<WeightCategory>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(50);
            e.HasIndex(c => c.Name).IsUnique();

            e.Property(c => c.MinWeightKg).HasPrecision(9, 2);
            e.HasIndex(c => c.MinWeightKg).IsUnique();

            e.Property(c => c.IconKey).IsRequired().HasMaxLength(30);

            e.ToTable(t => t.HasCheckConstraint(
                "CK_WeightCategory_MinWeight_NonNegative", "[MinWeightKg] >= 0"));

            e.HasData(
                new WeightCategory { Id = 1, Name = "Light",  MinWeightKg = 0m,    IconKey = "bike"  },
                new WeightCategory { Id = 2, Name = "Medium", MinWeightKg = 500m,  IconKey = "car"   },
                new WeightCategory { Id = 3, Name = "Heavy",  MinWeightKg = 2500m, IconKey = "truck" });
        });

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.Property(v => v.OwnerName).IsRequired().HasMaxLength(200);
            e.Property(v => v.WeightKg).HasPrecision(9, 2);

            e.HasOne(v => v.Manufacturer)
             .WithMany()
             .HasForeignKey(v => v.ManufacturerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.ToTable(t => t.HasCheckConstraint(
                "CK_Vehicle_Weight_Positive", "[WeightKg] > 0"));
        });
    }
}