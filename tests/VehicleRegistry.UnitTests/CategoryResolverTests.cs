using VehicleRegistry.Core.Domain;
using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.UnitTests;

public class CategoryResolverTests
{
    private static List<WeightCategory> DefaultCategories() =>
    [
        new() { Id = 1, Name = "Light",  MinWeightKg = 0,    IconKey = "bike"  },
        new() { Id = 2, Name = "Medium", MinWeightKg = 500,  IconKey = "car"   },
        new() { Id = 3, Name = "Heavy",  MinWeightKg = 2500, IconKey = "truck" },
    ];

    [Theory]
    [InlineData("0.01",    "Light")]
    [InlineData("499.99",  "Light")]
    [InlineData("500.00",  "Medium")]   // boundary belongs to the upper category
    [InlineData("2200",    "Medium")]
    [InlineData("2499.99", "Medium")]
    [InlineData("2500.00", "Heavy")]
    [InlineData("99999",   "Heavy")]
    public void Resolve_ReturnsExpectedCategory(string weight, string expected)
    {
        var result = CategoryResolver.Resolve(decimal.Parse(weight), DefaultCategories());
        Assert.Equal(expected, result.Name);
    }

    [Fact]
    public void Resolve_WhenHeavyStartsAt2000_VehicleOf2200BecomesHeavy()
    {
        var categories = DefaultCategories();
        categories.Single(c => c.Name == "Heavy").MinWeightKg = 2000;

        var result = CategoryResolver.Resolve(2200m, categories);

        Assert.Equal("Heavy", result.Name);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public void Resolve_NonPositiveWeight_Throws(string weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CategoryResolver.Resolve(decimal.Parse(weight), DefaultCategories()));
    }
}