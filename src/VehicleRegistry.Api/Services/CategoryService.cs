using System.Data;
using Microsoft.EntityFrameworkCore;
using VehicleRegistry.Api.Contracts;
using VehicleRegistry.Api.Data;
using VehicleRegistry.Core.Domain;
using VehicleRegistry.Core.Rules;

namespace VehicleRegistry.Api.Services;

public class CategoryService(AppDbContext db)
{
    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken ct)
    {
        var categories = await db.WeightCategories.AsNoTracking().ToListAsync(ct);
        return ToResponses(categories);
    }

    public async Task<OperationResult<CategoryResponse>> GetByIdAsync(int id, CancellationToken ct)
    {
        var categories = await db.WeightCategories.AsNoTracking().ToListAsync(ct);
        var match = ToResponses(categories).SingleOrDefault(c => c.Id == id);
        return match is null
            ? OperationResult<CategoryResponse>.NotFound()
            : OperationResult<CategoryResponse>.Success(match);
    }

    public async Task<OperationResult<CategoryResponse>> CreateAsync(CategoryRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var categories = await db.WeightCategories.ToListAsync(ct);

        var category = new WeightCategory();
        Apply(category, request);

        var proposed = categories.Append(category).ToList();
        var errors = CategoryRules.Validate(proposed);
        if (errors.Count > 0)
            return OperationResult<CategoryResponse>.Invalid(errors);

        db.WeightCategories.Add(category);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return OperationResult<CategoryResponse>.Success(
            ToResponses(proposed).Single(c => c.Id == category.Id));
    }

    public async Task<OperationResult<CategoryResponse>> UpdateAsync(int id, CategoryRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var categories = await db.WeightCategories.ToListAsync(ct);

        var category = categories.SingleOrDefault(c => c.Id == id);
        if (category is null)
            return OperationResult<CategoryResponse>.NotFound();

        Apply(category, request);

        var errors = CategoryRules.Validate(categories);
        if (errors.Count > 0)
            return OperationResult<CategoryResponse>.Invalid(errors);   // nothing saved

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return OperationResult<CategoryResponse>.Success(
            ToResponses(categories).Single(c => c.Id == id));
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var categories = await db.WeightCategories.ToListAsync(ct);

        var category = categories.SingleOrDefault(c => c.Id == id);
        if (category is null)
            return OperationResult<bool>.NotFound();

        var remaining = categories.Where(c => c.Id != id).ToList();
        var errors = CategoryRules.Validate(remaining);
        if (errors.Count > 0)
            return OperationResult<bool>.Invalid(errors);

        db.WeightCategories.Remove(category);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return OperationResult<bool>.Success(true);
    }

    private static void Apply(WeightCategory category, CategoryRequest request)
    {
        category.Name = request.Name?.Trim() ?? string.Empty;
        category.MinWeightKg = request.MinWeightKg;
        category.IconKey = request.IconKey?.Trim() ?? string.Empty;
    }

    private static List<CategoryResponse> ToResponses(IEnumerable<WeightCategory> categories)
    {
        var ordered = categories.OrderBy(c => c.MinWeightKg).ToList();
        return ordered
            .Select((c, i) => new CategoryResponse(
                c.Id, c.Name, c.MinWeightKg,
                i + 1 < ordered.Count ? ordered[i + 1].MinWeightKg : null,
                c.IconKey))
            .ToList();
    }
}