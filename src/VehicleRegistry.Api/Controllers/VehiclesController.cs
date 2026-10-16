using Microsoft.AspNetCore.Mvc;
using VehicleRegistry.Api.Contracts;
using VehicleRegistry.Api.Services;

namespace VehicleRegistry.Api.Controllers;

[Route("api/vehicles")]
public class VehiclesController(VehicleService vehicles) : ApiControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<VehicleResponse>> GetAll(
        [FromQuery] VehicleSortField sortBy = VehicleSortField.Owner,
        [FromQuery] SortDirection sortDir = SortDirection.Asc,
        CancellationToken ct = default) =>
        vehicles.GetAllAsync(sortBy, sortDir, ct);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleResponse>> GetById(int id, CancellationToken ct)
    {
        var result = await vehicles.GetByIdAsync(id, ct);
        return result.IsNotFound ? NotFound() : result.Value!;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VehicleResponse>> Create(CreateVehicleRequest request, CancellationToken ct)
    {
        var result = await vehicles.CreateAsync(request, ct);
        if (!result.IsSuccess)
            return ValidationErrors(result.Errors);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }
}