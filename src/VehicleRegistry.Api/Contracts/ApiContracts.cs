namespace VehicleRegistry.Api.Contracts;

public record ManufacturerResponse(int Id, string Name);

public record CreateVehicleRequest(
    string? OwnerName,
    int ManufacturerId,
    int YearOfManufacture,
    decimal WeightKg);

public record VehicleResponse(
    int Id,
    string OwnerName,
    int ManufacturerId,
    string ManufacturerName,
    int YearOfManufacture,
    decimal WeightKg,
    int CategoryId,
    string CategoryName,
    string CategoryIconKey);

public enum VehicleSortField { Owner, Manufacturer, Year, Weight }

public enum SortDirection { Asc, Desc }
public record CategoryRequest(string? Name, decimal MinWeightKg, string? IconKey);

public record CategoryResponse(
    int Id,
    string Name,
    decimal MinWeightKg,
    decimal? MaxWeightKg,   // exclusive; null = no upper limit
    string IconKey);