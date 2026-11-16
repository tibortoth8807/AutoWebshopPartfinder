namespace PartFinder.Core.Vin;

/// <summary>
/// World Manufacturer Identifier (VIN positions 1-3). A small, hand-picked subset for the demo -
/// a real system would load the full SAE list from the database.
/// </summary>
public static class WmiRegistry
{
    private static readonly Dictionary<string, (string Manufacturer, string Country)> Known = new(StringComparer.Ordinal)
    {
        ["WVW"] = ("Volkswagen", "Germany"),
        ["WV1"] = ("Volkswagen Commercial Vehicles", "Germany"),
        ["WV2"] = ("Volkswagen Commercial Vehicles", "Germany"),
        ["WAU"] = ("Audi", "Germany"),
        ["WBA"] = ("BMW", "Germany"),
        ["WDB"] = ("Mercedes-Benz", "Germany"),
        ["WDD"] = ("Mercedes-Benz", "Germany"),
        ["W0L"] = ("Opel", "Germany"),
        ["WF0"] = ("Ford", "Germany"),
        ["TMB"] = ("Škoda", "Czech Republic"),
        ["VSS"] = ("SEAT", "Spain"),
        ["VF1"] = ("Renault", "France"),
        ["VF3"] = ("Peugeot", "France"),
        ["VF7"] = ("Citroën", "France"),
        ["ZFA"] = ("Fiat", "Italy"),
        ["TSM"] = ("Suzuki", "Hungary"),
        ["JTD"] = ("Toyota", "Japan"),
        ["JHM"] = ("Honda", "Japan"),
        ["1HG"] = ("Honda", "United States"),
        ["1FA"] = ("Ford", "United States"),
        ["1G1"] = ("Chevrolet", "United States"),
        ["5YJ"] = ("Tesla", "United States"),
    };

    public static (string Manufacturer, string Country)? Find(string wmi) =>
        Known.TryGetValue(wmi, out var info) ? info : null;

    /// <summary>North American VINs (first character 1-5) must carry a valid check digit.</summary>
    public static bool UsesCheckDigit(string wmi) => wmi[0] is >= '1' and <= '5';
}
