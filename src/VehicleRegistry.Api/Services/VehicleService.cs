using Microsoft.EntityFrameworkCore;
using VehicleRegistry.Api.Contracts;
using VehicleRegistry.Api.Data;
using VehicleRegistry.Core.Domain;
using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.Api.Services;

public class VehicleService(AppDbContext db, TimeProvider time)
{
    public async Task<IReadOnlyList<VehicleResponse>> GetAllAsync(
        VehicleSortField sortBy, SortDirection direction, CancellationToken ct)
    {
        var desc = direction == SortDirection.Desc;
        IQueryable<Vehicle> query = db.Vehicles.AsNoTracking().Include(v => v.Manufacturer);

        IOrderedQueryable<Vehicle> ordered = sortBy switch
        {
            VehicleSortField.Manufacturer => desc
                ? query.OrderByDescending(v => v.Manufacturer!.Name)
                : query.OrderBy(v => v.Manufacturer!.Name),
            VehicleSortField.Year => desc
                ? query.OrderByDescending(v => v.YearOfManufacture)
                : query.OrderBy(v => v.YearOfManufacture),
            VehicleSortField.Weight => desc
                ? query.OrderByDescending(v => v.WeightKg)
                : query.OrderBy(v => v.WeightKg),
            _ => desc
                ? query.OrderByDescending(v => v.OwnerName)
                : query.OrderBy(v => v.OwnerName),
        };

        var vehicles = await ordered.ThenBy(v => v.Id).ToListAsync(ct);
        var categories = await db.WeightCategories.AsNoTracking().ToListAsync(ct);

        return vehicles.Select(v => ToResponse(v, categories)).ToList();
    }

    public async Task<OperationResult<VehicleResponse>> GetByIdAsync(int id, CancellationToken ct)
    {
        var vehicle = await db.Vehicles.AsNoTracking()
            .Include(v => v.Manufacturer)
            .SingleOrDefaultAsync(v => v.Id == id, ct);

        if (vehicle is null)
            return OperationResult<VehicleResponse>.NotFound();

        var categories = await db.WeightCategories.AsNoTracking().ToListAsync(ct);
        return OperationResult<VehicleResponse>.Success(ToResponse(vehicle, categories));
    }

    public async Task<OperationResult<VehicleResponse>> CreateAsync(
        CreateVehicleRequest request, CancellationToken ct)
    {
        var errors = VehicleRules.Validate(
            request.OwnerName, request.ManufacturerId, request.YearOfManufacture,
            request.WeightKg, time.GetLocalNow().Year).ToList();

        if (request.ManufacturerId > 0 &&
            !await db.Manufacturers.AnyAsync(m => m.Id == request.ManufacturerId, ct))
            errors.Add("The selected manufacturer does not exist.");

        if (errors.Count > 0)
            return OperationResult<VehicleResponse>.Invalid(errors);

        var vehicle = new Vehicle
        {
            OwnerName = request.OwnerName!.Trim(),
            ManufacturerId = request.ManufacturerId,
            YearOfManufacture = request.YearOfManufacture,
            WeightKg = request.WeightKg,
        };

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(ct);

        return await GetByIdAsync(vehicle.Id, ct);
    }

    private static VehicleResponse ToResponse(Vehicle v, IReadOnlyCollection<WeightCategory> categories)
    {
        var category = CategoryResolver.Resolve(v.WeightKg, categories);
        return new VehicleResponse(
            v.Id, v.OwnerName, v.ManufacturerId, v.Manufacturer!.Name,
            v.YearOfManufacture, v.WeightKg,
            category.Id, category.Name, category.IconKey);
    }
}