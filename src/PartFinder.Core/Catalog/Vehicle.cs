namespace PartFinder.Core.Catalog;

/// <summary>A concrete vehicle type at engine level, e.g. "Volkswagen Golf IV 1.9 TDI (2000-2004)".</summary>
public sealed class Vehicle
{
    public int Id { get; init; }

    public required string Make { get; init; }

    public required string Model { get; init; }

    /// <summary>Generation / platform name shown to the user, e.g. "Golf IV (1J)".</summary>
    public required string Generation { get; init; }

    public required string Engine { get; init; }

    public required FuelType Fuel { get; init; }

    public int PowerKw { get; init; }

    public int YearFrom { get; init; }

    public int YearTo { get; init; }

    // --- VIN matching rule (simplified): WMI + a model code at a fixed position ---

    /// <summary>VIN positions 1-3, e.g. "WVW".</summary>
    public required string VinWmi { get; init; }

    /// <summary>Model code inside the VIN, e.g. "1J" for VW Golf IV.</summary>
    public required string VinModelCode { get; init; }

    /// <summary>1-based VIN position where <see cref="VinModelCode"/> starts (VW group: 7, Opel: 5).</summary>
    public int VinModelCodePosition { get; init; }

    public List<Part> Parts { get; init; } = [];

    public string DisplayName => $"{Make} {Generation} {Engine} ({YearFrom}-{YearTo})";

    public bool MatchesVin(string vin, IReadOnlyCollection<int> possibleYears)
    {
        if (!vin.StartsWith(VinWmi, StringComparison.Ordinal))
        {
            return false;
        }

        var start = VinModelCodePosition - 1;
        if (start < 0 || start + VinModelCode.Length > vin.Length)
        {
            return false;
        }

        var codeMatches = string.CompareOrdinal(vin, start, VinModelCode, 0, VinModelCode.Length) == 0;
        return codeMatches && possibleYears.Any(year => year >= YearFrom && year <= YearTo);
    }
}

public enum FuelType
{
    Petrol,
    Diesel,
}
