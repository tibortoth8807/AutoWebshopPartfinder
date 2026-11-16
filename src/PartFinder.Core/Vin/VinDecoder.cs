namespace PartFinder.Core.Vin;

/// <summary>
/// Decodes a 17-character VIN (ISO 3779) without any external service:
/// structure, manufacturer (WMI), check digit and the candidate model years.
/// </summary>
public static class VinDecoder
{
    private const string AllowedCharacters = "0123456789ABCDEFGHJKLMNPRSTUVWXYZ"; // no I, O, Q
    private const string ModelYearCodes = "ABCDEFGHJKLMNPRSTVWXY123456789";    // 30-year cycle starting 1980
    private static readonly int[] Weights = [8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2];

    public static VinValidationResult Decode(string? input, int? currentYear = null)
    {
        var vin = Normalize(input);

        if (vin.Length != 17)
        {
            return VinValidationResult.Fail("A VIN must be exactly 17 characters long.");
        }

        var invalid = vin.FirstOrDefault(c => !AllowedCharacters.Contains(c));
        if (invalid != default)
        {
            return VinValidationResult.Fail(invalid is 'I' or 'O' or 'Q'
                ? $"The letter '{invalid}' is never used in a VIN (it looks like 1 or 0)."
                : $"Invalid character '{invalid}'.");
        }

        var wmi = vin[..3];
        var manufacturer = WmiRegistry.Find(wmi);

        var checkDigit = vin[8] == CalculateCheckDigit(vin)
            ? CheckDigitStatus.Valid
            : WmiRegistry.UsesCheckDigit(wmi) ? CheckDigitStatus.Invalid : CheckDigitStatus.NotUsed;

        if (checkDigit == CheckDigitStatus.Invalid)
        {
            return VinValidationResult.Fail("Check digit (9th character) does not match - probably a typo.");
        }

        var yearCode = vin[9];
        var years = PossibleModelYears(yearCode, currentYear ?? DateTime.UtcNow.Year);

        return VinValidationResult.Ok(new DecodedVin(
            vin,
            Wmi: wmi,
            Vds: vin[3..9],
            Vis: vin[9..],
            Manufacturer: manufacturer?.Manufacturer,
            Country: manufacturer?.Country,
            CheckDigit: checkDigit,
            ModelYearCode: yearCode,
            PossibleModelYears: years,
            PlantCode: vin[10].ToString(),
            SerialNumber: vin[11..]));
    }

    public static string Normalize(string? input) =>
        new((input ?? string.Empty).Where(c => !char.IsWhiteSpace(c) && c != '-').Select(char.ToUpperInvariant).ToArray());

    /// <summary>ISO 3779 / FMVSS 115 check digit: weighted sum of transliterated characters, modulo 11 ('X' = 10).</summary>
    public static char CalculateCheckDigit(string vin)
    {
        var sum = 0;
        for (var i = 0; i < 17; i++)
        {
            sum += Transliterate(vin[i]) * Weights[i];
        }

        var remainder = sum % 11;
        return remainder == 10 ? 'X' : (char)('0' + remainder);
    }

    /// <summary>
    /// The 10th character repeats every 30 years (e.g. '3' = 2003 or 2033), so the VIN alone gives candidates.
    /// Future years beyond next year are dropped; the vehicle catalog's production years pick the real one.
    /// </summary>
    public static IReadOnlyList<int> PossibleModelYears(char code, int currentYear)
    {
        var index = ModelYearCodes.IndexOf(code, StringComparison.Ordinal);
        if (index < 0)
        {
            return [];
        }

        return Enumerable.Range(0, 3)
            .Select(cycle => 1980 + index + (cycle * 30))
            .Where(year => year <= currentYear + 1)
            .OrderDescending()
            .ToList();
    }

    private static int Transliterate(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        'A' or 'J' => 1,
        'B' or 'K' or 'S' => 2,
        'C' or 'L' or 'T' => 3,
        'D' or 'M' or 'U' => 4,
        'E' or 'N' or 'V' => 5,
        'F' or 'W' => 6,
        'G' or 'P' or 'X' => 7,
        'H' or 'Y' => 8,
        'R' or 'Z' => 9,
        _ => 0,
    };
}
