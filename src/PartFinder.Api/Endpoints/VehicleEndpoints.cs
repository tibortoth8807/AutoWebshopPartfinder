using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartFinder.Infrastructure.Persistence;

namespace PartFinder.Api.Endpoints;

/// <summary>Classic "make → model → engine" vehicle selector, for users without the VIN at hand.</summary>
public static class VehicleEndpoints
{
    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vehicles").WithTags("Vehicles");

        group.MapGet("/makes", async ([FromServices] CatalogDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Vehicles.Select(v => v.Make).Distinct().OrderBy(m => m).ToListAsync(ct)));

        group.MapGet("/", async ([FromQuery] string? make, [FromQuery] string? model, [FromServices] CatalogDbContext db, CancellationToken ct) =>
        {
            var vehicles = await db.Vehicles
                .AsNoTracking()
                .Where(v => make == null || v.Make == make)
                .Where(v => model == null || v.Model == model)
                .OrderBy(v => v.Make).ThenBy(v => v.Model).ThenBy(v => v.Engine)
                .ToListAsync(ct);

            return Results.Ok(vehicles.Select(VehicleDto.From));
        });

        group.MapGet("/{id:int}", async (int id, [FromServices] CatalogDbContext db, CancellationToken ct) =>
            await db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct) is { } vehicle
                ? Results.Ok(VehicleDto.From(vehicle))
                : Results.NotFound());

        return app;
    }
}
