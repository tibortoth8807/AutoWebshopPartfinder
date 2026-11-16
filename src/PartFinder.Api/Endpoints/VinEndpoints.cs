using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartFinder.Core.Vin;
using PartFinder.Infrastructure.Nhtsa;
using PartFinder.Infrastructure.Persistence;

namespace PartFinder.Api.Endpoints;

public static class VinEndpoints
{
    public static IEndpointRouteBuilder MapVinEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/vin/WVWZZZ1JZ3W386752?enrich=true
        app.MapGet("/api/vin/{vin}", LookupAsync).WithTags("VIN").WithName("LookupVin");
        return app;
    }

    private static async Task<IResult> LookupAsync(
        string vin,
        [FromQuery] bool? enrich,
        [FromServices] CatalogDbContext db,
        [FromServices] NhtsaVinClient nhtsa,
        [FromServices] IConfiguration configuration,
        CancellationToken ct)
    {
        var result = VinDecoder.Decode(vin);
        if (!result.IsValid || result.Decoded is null)
        {
            return Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest, title: "Invalid VIN");
        }

        var decoded = result.Decoded;

        // Start the optional external lookup in parallel with the database query.
        var useNhtsa = (enrich ?? true) && configuration.GetValue("Nhtsa:Enabled", true);
        var nhtsaTask = useNhtsa ? nhtsa.DecodeAsync(decoded.Vin, ct) : Task.FromResult<NhtsaVinInfo?>(null);

        var candidates = await db.Vehicles
            .AsNoTracking()
            .Where(v => v.VinWmi == decoded.Wmi)
            .ToListAsync(ct);

        var matches = candidates
            .Where(v => v.MatchesVin(decoded.Vin, decoded.PossibleModelYears))
            .OrderBy(v => v.Engine)
            .Select(VehicleDto.From)
            .ToList();

        return Results.Ok(new VinLookupResponse(decoded, matches, await nhtsaTask));
    }
}
