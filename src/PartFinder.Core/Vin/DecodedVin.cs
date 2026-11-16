namespace PartFinder.Core.Vin;

public enum CheckDigitStatus
{
    /// <summary>Position 9 matches the ISO 3779 / FMVSS 115 check digit.</summary>
    Valid,

    /// <summary>Position 9 does not match. Mandatory for North America, so this means a typo or a fake VIN there.</summary>
    Invalid,

    /// <summary>European and most other manufacturers don't use a check digit (often 'Z' or a free character).</summary>
    NotUsed,
}

/// <summary>Everything that can be read from the 17 characters themselves, without any external database.</summary>
public sealed record DecodedVin(
    string Vin,
    string Wmi,
    string Vds,
    string Vis,
    string? Manufacturer,
    string? Country,
    CheckDigitStatus CheckDigit,
    char ModelYearCode,
    IReadOnlyList<int> PossibleModelYears,
    string? PlantCode,
    string SerialNumber);

public sealed record VinValidationResult(bool IsValid, string? Error, DecodedVin? Decoded)
{
    public static VinValidationResult Fail(string error) => new(false, error, null);

    public static VinValidationResult Ok(DecodedVin decoded) => new(true, null, decoded);
}
