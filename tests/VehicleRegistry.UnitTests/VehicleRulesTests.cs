using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.UnitTests;

public class VehicleRulesTests
{
    private const int CurrentYear = 2026;

    [Fact]
    public void Validate_ValidVehicle_HasNoErrors()
    {
        var errors = VehicleRules.Validate("John Smith", 1, 2019, 1850.75m, CurrentYear);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingOwner_ReturnsError(string? owner)
    {
        var errors = VehicleRules.Validate(owner, 1, 2019, 1000m, CurrentYear);
        Assert.Contains(errors, e => e.Contains("Owner"));
    }

    [Fact]
    public void Validate_MissingManufacturer_ReturnsError()
    {
        var errors = VehicleRules.Validate("Jane", 0, 2019, 1000m, CurrentYear);
        Assert.Contains(errors, e => e.Contains("Manufacturer"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1885)]
    [InlineData(2028)]
    public void Validate_UnsensibleYear_ReturnsError(int year)
    {
        var errors = VehicleRules.Validate("Jane", 1, year, 1000m, CurrentYear);
        Assert.Contains(errors, e => e.Contains("Year"));
    }

    [Theory]
    [InlineData(1886)]
    [InlineData(2027)]
    public void Validate_BoundaryYears_AreAccepted(int year)
    {
        var errors = VehicleRules.Validate("Jane", 1, year, 1000m, CurrentYear);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public void Validate_NonPositiveWeight_ReturnsError(string weight)
    {
        var errors = VehicleRules.Validate("Jane", 1, 2019, decimal.Parse(weight), CurrentYear);
        Assert.Contains(errors, e => e.Contains("positive"));
    }

    [Fact]
    public void Validate_ThreeDecimalPlaces_ReturnsError()
    {
        var errors = VehicleRules.Validate("Jane", 1, 2019, 500.123m, CurrentYear);
        Assert.Contains(errors, e => e.Contains("2 decimal places"));
    }
}