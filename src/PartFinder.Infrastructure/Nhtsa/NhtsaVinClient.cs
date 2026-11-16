using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace PartFinder.Infrastructure.Nhtsa;

/// <summary>What the public NHTSA vPIC service knows about a VIN. Strong for US-market cars, weak for EU ones.</summary>
public sealed record NhtsaVinInfo(string? Make, string? Model, string? ModelYear, string? BodyClass, string? FuelType, string? ErrorText);

/// <summary>
/// Typed client for https://vpic.nhtsa.dot.gov (free, no API key).
/// Optional enrichment: the shop must keep working when the service is slow or down,
/// so failures are logged and turned into <c>null</c> (resilience pipeline handles retries/timeouts).
/// </summary>
public sealed class NhtsaVinClient(HttpClient http, ILogger<NhtsaVinClient> logger)
{
    public async Task<NhtsaVinInfo?> DecodeAsync(string vin, CancellationToken ct)
    {
        try
        {
            var response = await http.GetFromJsonAsync<VpicResponse>(
                $"api/vehicles/DecodeVinValues/{Uri.EscapeDataString(vin)}?format=json", ct);

            var row = response?.Results?.FirstOrDefault();
            return row is null
                ? null
                : new NhtsaVinInfo(Blank(row.Make), Blank(row.Model), Blank(row.ModelYear), Blank(row.BodyClass), Blank(row.FuelTypePrimary), Blank(row.ErrorText));
        }
        // Any failure (HTTP error, timeout, open circuit, bad JSON) degrades to "no extra info",
        // unless the caller itself cancelled.
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "NHTSA vPIC lookup failed for VIN {Vin}", vin);
            return null;
        }
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record VpicResponse([property: JsonPropertyName("Results")] List<VpicRow>? Results);

    private sealed record VpicRow(
        [property: JsonPropertyName("Make")] string? Make,
        [property: JsonPropertyName("Model")] string? Model,
        [property: JsonPropertyName("ModelYear")] string? ModelYear,
        [property: JsonPropertyName("BodyClass")] string? BodyClass,
        [property: JsonPropertyName("FuelTypePrimary")] string? FuelTypePrimary,
        [property: JsonPropertyName("ErrorText")] string? ErrorText);
}
