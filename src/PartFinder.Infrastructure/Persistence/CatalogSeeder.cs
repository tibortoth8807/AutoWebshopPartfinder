using Microsoft.EntityFrameworkCore;
using PartFinder.Core.Catalog;

namespace PartFinder.Infrastructure.Persistence;

/// <summary>
/// Demo catalog: 3 model families, 6 engine variants, ~3 categories and a few hundred generated parts.
/// All brands, article numbers and prices are fictional. VIN rules are simplified for the demo.
/// </summary>
public static class CatalogSeeder
{
    private sealed record PartTemplate(
        string CategorySlug, string Name, string? Position, string? DiagramKey, decimal BasePrice, FuelType? OnlyFor = null);

    private static readonly PartTemplate[] Templates =
    [
        new("fektarcsa", "Féktárcsa, hűtött", "Első tengely", "brake-disc-front", 9_800m),
        new("fektarcsa", "Féktárcsa, tömör", "Hátsó tengely", "brake-disc-rear", 7_400m),
        new("fekbetet", "Fékbetét készlet", "Első tengely", "brake-pad-front", 6_900m),
        new("fekbetet", "Fékbetét készlet", "Hátsó tengely", "brake-pad-rear", 5_600m),
        new("feknyereg", "Féknyereg, felújított", "Első tengely", "brake-caliper-front", 24_500m),
        new("fekcso", "Fékcső", "Első tengely", "brake-hose", 2_900m),
        new("lengescsillapito", "Lengéscsillapító, gáztöltésű", "Első tengely", "shock-absorber", 16_900m),
        new("tekercsrugo", "Tekercsrugó", "Első tengely", "coil-spring", 11_200m),
        new("lengokar", "Lengőkar, gömbfejjel", "Első tengely", "control-arm", 13_400m),
        new("stabilizator-palca", "Stabilizátor pálca", "Első tengely", "stabilizer-link", 3_800m),
        new("toronycsapagy", "Toronycsapágy készlet", "Első tengely", "strut-mount", 8_700m),
        new("olajszuro", "Olajszűrő", null, null, 2_400m),
        new("levegoszuro", "Levegőszűrő", null, null, 3_600m),
        new("pollenszuro", "Pollenszűrő, aktívszenes", null, null, 4_200m),
        new("uzemanyagszuro", "Gázolajszűrő", null, null, 6_300m, FuelType.Diesel),
        new("uzemanyagszuro", "Benzinszűrő", null, null, 3_900m, FuelType.Petrol),
    ];

    private static readonly (string Name, decimal PriceFactor)[] Brands =
    [
        ("Stopline", 1.00m),
        ("Ridemax", 1.18m),
        ("NordParts", 0.86m),
    ];

    public static async Task SeedAsync(CatalogDbContext db, CancellationToken ct = default)
    {
        if (await db.Parts.AnyAsync(ct))
        {
            return;
        }

        var categories = CreateCategories();
        db.Categories.AddRange(categories);

        var vehicles = CreateVehicles();
        db.Vehicles.AddRange(vehicles);

        var leafBySlug = categories.SelectMany(c => c.Children).ToDictionary(c => c.Slug);
        var families = vehicles.GroupBy(v => v.Generation).ToList();

        var sku = 10_000;
        for (var f = 0; f < families.Count; f++)
        {
            var family = families[f];
            var familyFactor = 1m + (f * 0.07m);

            foreach (var template in Templates)
            {
                var fits = family.Where(v => template.OnlyFor is null || v.Fuel == template.OnlyFor).ToList();
                if (fits.Count == 0)
                {
                    continue;
                }

                foreach (var (brand, priceFactor) in Brands)
                {
                    sku++;
                    db.Parts.Add(new Part
                    {
                        Sku = $"PF-{sku}",
                        Name = template.Name,
                        Brand = brand,
                        Description = $"{template.Name} - {family.Key}. Demo termék, kitalált adatokkal.",
                        Category = leafBySlug[template.CategorySlug],
                        Position = template.Position,
                        DiagramKey = template.DiagramKey,
                        Price = Math.Round(template.BasePrice * familyFactor * priceFactor / 10m) * 10m,
                        Stock = (sku * 7) % 25,
                        Vehicles = [.. fits], // own list per part (EF tracks collections per entity)
                    });
                }
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static List<Category> CreateCategories() =>
    [
        new()
        {
            Name = "Fékrendszer", Slug = "fekrendszer", DiagramId = "brakes", SortOrder = 1,
            Children =
            [
                new() { Name = "Féktárcsa", Slug = "fektarcsa", SortOrder = 1 },
                new() { Name = "Fékbetét", Slug = "fekbetet", SortOrder = 2 },
                new() { Name = "Féknyereg", Slug = "feknyereg", SortOrder = 3 },
                new() { Name = "Fékcső", Slug = "fekcso", SortOrder = 4 },
            ],
        },
        new()
        {
            Name = "Futómű", Slug = "futomu", DiagramId = "suspension", SortOrder = 2,
            Children =
            [
                new() { Name = "Lengéscsillapító", Slug = "lengescsillapito", SortOrder = 1 },
                new() { Name = "Tekercsrugó", Slug = "tekercsrugo", SortOrder = 2 },
                new() { Name = "Lengőkar", Slug = "lengokar", SortOrder = 3 },
                new() { Name = "Stabilizátor pálca", Slug = "stabilizator-palca", SortOrder = 4 },
                new() { Name = "Toronycsapágy", Slug = "toronycsapagy", SortOrder = 5 },
            ],
        },
        new()
        {
            Name = "Szűrők", Slug = "szurok", SortOrder = 3,
            Children =
            [
                new() { Name = "Olajszűrő", Slug = "olajszuro", SortOrder = 1 },
                new() { Name = "Levegőszűrő", Slug = "levegoszuro", SortOrder = 2 },
                new() { Name = "Pollenszűrő", Slug = "pollenszuro", SortOrder = 3 },
                new() { Name = "Üzemanyagszűrő", Slug = "uzemanyagszuro", SortOrder = 4 },
            ],
        },
    ];

    private static List<Vehicle> CreateVehicles() =>
    [
        Vw("1.6", FuelType.Petrol, 77, 2000, 2004),
        Vw("1.9 TDI", FuelType.Diesel, 74, 1998, 2004),
        Skoda("1.6 MPI", FuelType.Petrol, 75, 2004, 2010),
        Skoda("1.9 TDI", FuelType.Diesel, 77, 2004, 2010),
        Opel("1.6", FuelType.Petrol, 77, 2004, 2010),
        Opel("1.7 CDTI", FuelType.Diesel, 74, 2004, 2010),
    ];

    private static Vehicle Vw(string engine, FuelType fuel, int kw, int from, int to) => new()
    {
        Make = "Volkswagen", Model = "Golf", Generation = "Golf IV (1J)", Engine = engine, Fuel = fuel, PowerKw = kw,
        YearFrom = from, YearTo = to, VinWmi = "WVW", VinModelCode = "1J", VinModelCodePosition = 7,
    };

    private static Vehicle Skoda(string engine, FuelType fuel, int kw, int from, int to) => new()
    {
        Make = "Škoda", Model = "Octavia", Generation = "Octavia II (1Z)", Engine = engine, Fuel = fuel, PowerKw = kw,
        YearFrom = from, YearTo = to, VinWmi = "TMB", VinModelCode = "1Z", VinModelCodePosition = 7,
    };

    private static Vehicle Opel(string engine, FuelType fuel, int kw, int from, int to) => new()
    {
        Make = "Opel", Model = "Astra", Generation = "Astra H", Engine = engine, Fuel = fuel, PowerKw = kw,
        YearFrom = from, YearTo = to, VinWmi = "W0L", VinModelCode = "AH", VinModelCodePosition = 5,
    };
}
