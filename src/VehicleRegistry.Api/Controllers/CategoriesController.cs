using Microsoft.AspNetCore.Mvc;
using VehicleRegistry.Api.Contracts;
using VehicleRegistry.Api.Services;
using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.Api.Controllers;

[Route("api/categories")]
public class CategoriesController(CategoryService categories) : ApiControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CategoryResponse>> GetAll(CancellationToken ct) =>
        categories.GetAllAsync(ct);

    [HttpGet("icons")]
    public IReadOnlyList<string> GetIcons() => CategoryIcons.All;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id, CancellationToken ct)
    {
        var result = await categories.GetByIdAsync(id, ct);
        return result.IsNotFound ? NotFound() : result.Value!;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request, CancellationToken ct)
    {
        var result = await categories.CreateAsync(request, ct);
        if (!result.IsSuccess)
            return ValidationErrors(result.Errors);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, CategoryRequest request, CancellationToken ct)
    {
        var result = await categories.UpdateAsync(id, request, ct);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return ValidationErrors(result.Errors);
        return result.Value!;
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await categories.DeleteAsync(id, ct);
        if (result.IsNotFound) return NotFound();
        if (!result.IsSuccess) return ValidationErrors(result.Errors);
        return NoContent();
    }
}