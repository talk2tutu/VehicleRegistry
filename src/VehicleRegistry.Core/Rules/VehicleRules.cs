namespace VehicleRegistry.Core.Rules;

public static class VehicleRules
{
    public const int MinYear = 1886;                    // first petrol car
    public const int MaxOwnerNameLength = 200;
    public const decimal MaxWeightKg = 9_999_999.99m;   // fits decimal(9,2)

    public static IReadOnlyList<string> Validate(
        string? ownerName, int manufacturerId, int yearOfManufacture,
        decimal weightKg, int currentYear)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ownerName))
            errors.Add("Owner's name is required.");
        else if (ownerName.Trim().Length > MaxOwnerNameLength)
            errors.Add($"Owner's name must be {MaxOwnerNameLength} characters or fewer.");

        if (manufacturerId <= 0)
            errors.Add("Manufacturer is required.");

        if (yearOfManufacture < MinYear || yearOfManufacture > currentYear + 1)
            errors.Add($"Year of manufacture must be between {MinYear} and {currentYear + 1}.");

        if (weightKg <= 0)
            errors.Add("Weight must be a positive number.");
        else if (weightKg > MaxWeightKg)
            errors.Add($"Weight cannot exceed {MaxWeightKg:N2} kg.");

        if (decimal.Round(weightKg, 2) != weightKg)
            errors.Add("Weight can have at most 2 decimal places.");

        return errors;
    }
}