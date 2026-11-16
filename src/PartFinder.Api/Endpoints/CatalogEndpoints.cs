using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartFinder.Core.Catalog;
using PartFinder.Infrastructure.Persistence;

namespace PartFinder.Api.Endpoints;

public static class CatalogEndpoints
{
    private const int MaxPageSize = 100;

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").WithTags("Catalog");

        api.MapGet("/categories", GetCategoriesAsync);
        api.MapGet("/parts", SearchPartsAsync);
        api.MapGet("/parts/{id:int}", GetPartAsync);

        return app;
    }

    private static async Task<IResult> GetCategoriesAsync([FromServices] CatalogDbContext db, CancellationToken ct)
    {
        var all = await db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync(ct);

        CategoryDto ToCategoryDto(Category c) => new(
            c.Id, c.Name, c.Slug, c.DiagramId,
            all.Where(child => child.ParentId == c.Id).Select(ToCategoryDto).ToList());

        return Results.Ok(all.Where(c => c.ParentId == null).Select(ToCategoryDto).ToList());
    }

    /// <summary>
    /// GET /api/parts?vehicleId=3&amp;category=fekrendszer&amp;diagramKey=brake-disc-front&amp;q=tárcsa&amp;sort=price&amp;page=1&amp;pageSize=24
    /// <c>category</c> can be a top-level or a leaf slug. <c>sort</c>: name (default), price, -price.
    /// </summary>
    private static async Task<IResult> SearchPartsAsync(
        [FromQuery] int? vehicleId,
        [FromQuery] string? category,
        [FromQuery] string? diagramKey,
        [FromQuery] string? q,
        [FromQuery] string? sort,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] CatalogDbContext db,
        CancellationToken ct)
    {
        var currentPage = Math.Max(page ?? 1, 1);
        var size = Math.Clamp(pageSize ?? 24, 1, MaxPageSize);

        IQueryable<Part> query = db.Parts.AsNoTracking().Include(p => p.Category);

        if (vehicleId is not null)
        {
            query = query.Where(p => p.Vehicles.Any(v => v.Id == vehicleId));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var categoryIds = await db.Categories
                .Where(c => c.Slug == category || (c.Parent != null && c.Parent.Slug == category))
                .Select(c => c.Id)
                .ToListAsync(ct);
            query = query.Where(p => categoryIds.Contains(p.CategoryId));
        }

        if (!string.IsNullOrWhiteSpace(diagramKey))
        {
            query = query.Where(p => p.DiagramKey == diagramKey);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var pattern = $"%{q.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, pattern) || EF.Functions.ILike(p.Brand, pattern) || EF.Functions.ILike(p.Sku, pattern));
        }

        query = sort switch
        {
            "price" => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            "-price" => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            _ => query.OrderBy(p => p.Name).ThenBy(p => p.Price).ThenBy(p => p.Id),
        };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((currentPage - 1) * size).Take(size).ToListAsync(ct);

        return Results.Ok(new PagedResult<PartDto>(items.Select(ToDto).ToList(), total, currentPage, size));
    }

    private static async Task<IResult> GetPartAsync(int id, [FromServices] CatalogDbContext db, CancellationToken ct)
    {
        var part = await db.Parts.AsNoTracking().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);
        return part is null ? Results.NotFound() : Results.Ok(ToDto(part));
    }

    private static PartDto ToDto(Part p) => new(
        p.Id, p.Sku, p.Name, p.Brand, p.Description,
        p.Category?.Slug ?? string.Empty, p.Category?.Name ?? string.Empty,
        p.Price, p.Stock, p.Position, p.DiagramKey);
}
