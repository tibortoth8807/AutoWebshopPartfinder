namespace PartFinder.Core.Catalog;

/// <summary>Two-level category tree, e.g. "Fékrendszer" → "Féktárcsa".</summary>
public sealed class Category
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public required string Slug { get; init; }

    public int? ParentId { get; init; }

    public Category? Parent { get; init; }

    public List<Category> Children { get; init; } = [];

    /// <summary>Exploded-view diagram for a top-level category ("brakes", "suspension"), if there is one.</summary>
    public string? DiagramId { get; init; }

    public int SortOrder { get; init; }
}
