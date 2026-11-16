using PartFinder.Core.Catalog;
using PartFinder.Core.Vin;
using PartFinder.Infrastructure.Nhtsa;

namespace PartFinder.Api.Endpoints;

public sealed record VehicleDto(
    int Id, string Make, string Model, string Generation, string Engine, FuelType Fuel, int PowerKw, int YearFrom, int YearTo, string DisplayName)
{
    public static VehicleDto From(Vehicle v) =>
        new(v.Id, v.Make, v.Model, v.Generation, v.Engine, v.Fuel, v.PowerKw, v.YearFrom, v.YearTo, v.DisplayName);
}

/// <summary>
/// VIN search result. <c>Matches</c> may contain several engine variants - the VIN alone usually
/// doesn't tell the engine for EU cars, so the user picks the exact one.
/// </summary>
public sealed record VinLookupResponse(DecodedVin Vin, IReadOnlyList<VehicleDto> Matches, NhtsaVinInfo? Nhtsa);

public sealed record CategoryDto(int Id, string Name, string Slug, string? DiagramId, IReadOnlyList<CategoryDto> Children);

public sealed record PartDto(
    int Id,
    string Sku,
    string Name,
    string Brand,
    string? Description,
    string CategorySlug,
    string CategoryName,
    decimal Price,
    int Stock,
    string? Position,
    string? DiagramKey);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
