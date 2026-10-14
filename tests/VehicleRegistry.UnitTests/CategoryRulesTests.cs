using VehicleRegistry.Core.Domain;
using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.UnitTests;

public class CategoryRulesTests
{
    private static List<WeightCategory> DefaultCategories() =>
    [
        new() { Id = 1, Name = "Light",  MinWeightKg = 0,    IconKey = "bike"  },
        new() { Id = 2, Name = "Medium", MinWeightKg = 500,  IconKey = "car"   },
        new() { Id = 3, Name = "Heavy",  MinWeightKg = 2500, IconKey = "truck" },
    ];

    [Fact]
    public void Validate_DefaultCategories_IsValid()
    {
        var errors = CategoryRules.Validate(DefaultCategories());

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NoCategoryStartsAtZero_ReturnsGapError()
    {
        // Arrange: Light now starts at 100, so 0–99.99 kg has no category (a gap)
        var categories = DefaultCategories();
        categories.Single(c => c.Name == "Light").MinWeightKg = 100;

        // Act
        var errors = CategoryRules.Validate(categories);

        // Assert
        Assert.Contains(errors, e => e.Contains("0 kg"));
    }

    [Fact]
    public void Validate_TwoCategoriesSameStart_ReturnsOverlapError()
    {
        // Arrange: Heavy now also starts at 500, so 500 kg would match both Medium and Heavy
        var categories = DefaultCategories();
        categories.Single(c => c.Name == "Heavy").MinWeightKg = 500;

        // Act
        var errors = CategoryRules.Validate(categories);

        // Assert
        Assert.Contains(errors, e => e.Contains("overlap"));
    }

    [Fact]
    public void Validate_EmptyList_ReturnsError()
    {
        // Arrange: no categories at all, so no weight could be categorised
        var categories = new List<WeightCategory>();

        // Act
        var errors = CategoryRules.Validate(categories);

        // Assert
        Assert.Contains(errors, e => e.Contains("At least one"));
    }

    [Fact]
    public void Validate_MissingIcon_ReturnsError()
    {
        // Arrange: Medium has no icon
        var categories = DefaultCategories();
        categories.Single(c => c.Name == "Medium").IconKey = "";

        // Act
        var errors = CategoryRules.Validate(categories);

        // Assert
        Assert.Contains(errors, e => e.Contains("icon"));
    }
}