using VehicleRegistry.Core.Domain;

namespace VehicleRegistry.Core.Rules;

public static class CategoryRules
{
    public const int MaxDecimalPlaces = 2;
    public const int MaxNameLength = 50;

    public static IReadOnlyList<string> Validate(IReadOnlyCollection<WeightCategory> categories)
    {
        var errors = new List<string>();

        if (categories.Count == 0)
        {
            errors.Add("At least one category is required.");
            return errors;
        }

        foreach (var c in categories)
        {
            if (string.IsNullOrWhiteSpace(c.Name))
                errors.Add("Category name is required.");
            else if (c.Name.Trim().Length > MaxNameLength)
                errors.Add($"Category name '{c.Name}' must be {MaxNameLength} characters or fewer.");

            if (string.IsNullOrWhiteSpace(c.IconKey))
                errors.Add($"Category '{c.Name}' must have an icon.");
            else if (!CategoryIcons.IsValid(c.IconKey))
                errors.Add($"Category '{c.Name}' has an unknown icon '{c.IconKey}'.");

            if (c.MinWeightKg < 0)
                errors.Add($"Category '{c.Name}' cannot start below 0 kg.");
            if (decimal.Round(c.MinWeightKg, MaxDecimalPlaces) != c.MinWeightKg)
                errors.Add($"Category '{c.Name}' starting weight can have at most 2 decimal places.");
        }

        if (categories.Count(c => c.MinWeightKg == 0) != 1)
            errors.Add("Exactly one category must start at 0 kg so that every weight is covered.");

        var duplicateStarts = categories.GroupBy(c => c.MinWeightKg).Where(g => g.Count() > 1);
        foreach (var g in duplicateStarts)
            errors.Add($"More than one category starts at {g.Key} kg; ranges would overlap.");

        var duplicateNames = categories
            .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);
        foreach (var g in duplicateNames)
            errors.Add($"Category name '{g.Key}' is used more than once.");

        return errors;
    }
}