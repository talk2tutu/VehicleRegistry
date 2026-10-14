using VehicleRegistry.Core.Domain;

namespace VehicleRegistry.Core.Rules;

public static class CategoryResolver
{
    public static WeightCategory Resolve(decimal weightKg, IEnumerable<WeightCategory> categories)
    {
        if (weightKg <= 0)
            throw new ArgumentOutOfRangeException(nameof(weightKg), "Weight must be positive.");

        return categories
            .Where(c => c.MinWeightKg <= weightKg)
            .OrderByDescending(c => c.MinWeightKg)
            .FirstOrDefault()
            ?? throw new InvalidOperationException(
                "No category covers this weight. The category configuration is invalid.");
    }
}