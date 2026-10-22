namespace PickupPoint.Core.Domain;

/// <summary>Товар із довідника пункту видачі.</summary>
public sealed class Product
{
    public string Sku { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public int Stock { get; init; }
}
