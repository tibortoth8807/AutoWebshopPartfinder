namespace PartFinder.Core.Catalog;

public sealed class Part
{
    public int Id { get; init; }

    /// <summary>Own article number, e.g. "PF-10234".</summary>
    public required string Sku { get; init; }

    public required string Name { get; init; }

    /// <summary>Fictional brand names only - this is demo data.</summary>
    public required string Brand { get; init; }

    public string? Description { get; init; }

    public int CategoryId { get; init; }

    public Category? Category { get; init; }

    /// <summary>Gross price in HUF.</summary>
    public decimal Price { get; init; }

    public int Stock { get; init; }

    /// <summary>Front / rear axle etc. Shown as a badge in the UI.</summary>
    public string? Position { get; init; }

    /// <summary>Hotspot id on the exploded-view diagram, e.g. "brake-disc-front".</summary>
    public string? DiagramKey { get; init; }

    /// <summary>Fitment: vehicles this part fits (many-to-many).</summary>
    public List<Vehicle> Vehicles { get; init; } = [];
}
