namespace VehicleRegistry.Core.Rules;

public static class CategoryIcons
{
    public static readonly IReadOnlyList<string> All =
        ["bike", "car", "van", "truck", "bus", "tractor"];

    public static bool IsValid(string? iconKey) =>
        iconKey is not null && All.Contains(iconKey);
}