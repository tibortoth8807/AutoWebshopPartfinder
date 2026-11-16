using PartFinder.Core.Catalog;
using PartFinder.Core.Vin;

namespace PartFinder.Tests;

public sealed class VinDecoderTests
{
    [Theory]
    [InlineData("1M8GDM9AXKP042788", 'X')] // textbook example, check digit 'X' (= 10)
    [InlineData("1HGCM82633A004352", '3')] // Honda Accord (US)
    public void Calculates_check_digit(string vin, char expected) =>
        Assert.Equal(expected, VinDecoder.CalculateCheckDigit(vin));

    [Fact]
    public void Us_vin_with_valid_check_digit_is_valid()
    {
        var result = VinDecoder.Decode("1HGCM82633A004352", currentYear: 2026);

        Assert.True(result.IsValid);
        Assert.Equal(CheckDigitStatus.Valid, result.Decoded!.CheckDigit);
        Assert.Equal("Honda", result.Decoded.Manufacturer);
    }

    [Fact]
    public void Us_vin_with_wrong_check_digit_is_rejected()
    {
        var result = VinDecoder.Decode("1HGCM82643A004352");

        Assert.False(result.IsValid);
        Assert.Contains("Check digit", result.Error);
    }

    [Fact]
    public void European_vin_without_check_digit_is_accepted()
    {
        var result = VinDecoder.Decode("WVWZZZ1JZ3W386752", currentYear: 2026);

        Assert.True(result.IsValid);
        var vin = result.Decoded!;
        Assert.Equal(CheckDigitStatus.NotUsed, vin.CheckDigit);
        Assert.Equal("Volkswagen", vin.Manufacturer);
        Assert.Equal("Germany", vin.Country);
        Assert.Equal("WVW", vin.Wmi);
        Assert.Equal("386752", vin.SerialNumber);
    }

    [Theory]
    [InlineData(" wvwzzz1jz3w386752 ", "WVWZZZ1JZ3W386752")]
    [InlineData("WVW-ZZZ1JZ-3W386752", "WVWZZZ1JZ3W386752")]
    public void Input_is_normalized(string input, string expected) =>
        Assert.Equal(expected, VinDecoder.Normalize(input));

    [Theory]
    [InlineData("WVWZZZ1JZ3W38675", "17 characters")]
    [InlineData("WVWZZZ1JZ3WO86752", "'O'")]
    [InlineData("WVWZZZ1JZ3W38675!", "Invalid character")]
    [InlineData(null, "17 characters")]
    public void Invalid_input_gives_readable_error(string? input, string expectedFragment)
    {
        var result = VinDecoder.Decode(input);

        Assert.False(result.IsValid);
        Assert.Contains(expectedFragment, result.Error);
    }

    [Theory]
    [InlineData('3', 2026, new[] { 2003 })]          // 2033 is in the future
    [InlineData('K', 2026, new[] { 2019, 1989 })]
    [InlineData('A', 2026, new[] { 2010, 1980 })]
    [InlineData('Y', 2029, new[] { 2030, 2000 })]    // next model year is allowed
    public void Model_year_code_gives_all_plausible_years(char code, int currentYear, int[] expected) =>
        Assert.Equal(expected, VinDecoder.PossibleModelYears(code, currentYear));

    [Fact]
    public void Unknown_model_year_code_gives_no_years() =>
        Assert.Empty(VinDecoder.PossibleModelYears('Z', 2026));
}

public sealed class VehicleVinMatchingTests
{
    private static Vehicle GolfIv(int from, int to) => new()
    {
        Make = "Volkswagen", Model = "Golf", Generation = "Golf IV (1J)", Engine = "1.9 TDI", Fuel = FuelType.Diesel,
        YearFrom = from, YearTo = to, VinWmi = "WVW", VinModelCode = "1J", VinModelCodePosition = 7,
    };

    [Fact]
    public void Matches_on_wmi_model_code_and_year() =>
        Assert.True(GolfIv(1998, 2004).MatchesVin("WVWZZZ1JZ3W386752", [2003]));

    [Fact]
    public void Year_outside_production_range_does_not_match() =>
        Assert.False(GolfIv(1998, 2002).MatchesVin("WVWZZZ1JZ3W386752", [2003]));

    [Fact]
    public void Different_model_code_does_not_match() =>
        Assert.False(GolfIv(1998, 2004).MatchesVin("WVWZZZ1KZ3W386752", [2003]));

    [Fact]
    public void Year_ambiguity_is_resolved_by_production_range() =>
        Assert.True(GolfIv(1998, 2004).MatchesVin("WVWZZZ1JZ3W386752", [2033, 2003]));
}
