namespace VehicleRegistry.Core.Domain;

public class WeightCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinWeightKg { get; set; }
    public string IconKey { get; set; } = string.Empty;
}