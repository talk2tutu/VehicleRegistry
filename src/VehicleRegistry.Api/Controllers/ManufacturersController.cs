using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleRegistry.Api.Contracts;
using VehicleRegistry.Api.Data;

namespace VehicleRegistry.Api.Controllers;

[Route("api/manufacturers")]
public class ManufacturersController(AppDbContext db) : ApiControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<ManufacturerResponse>> GetAll(CancellationToken ct) =>
        await db.Manufacturers.AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new ManufacturerResponse(m.Id, m.Name))
            .ToListAsync(ct);
}