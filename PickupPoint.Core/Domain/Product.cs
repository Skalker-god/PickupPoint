namespace PickupPoint.Core.Domain;

/// <summary>Товар із довідника пункту видачі.</summary>
public sealed class Product
{
    /// <summary>Gets артикул товару.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Gets назва товару.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets ціна за одиницю, грн.</summary>
    public decimal Price { get; init; }

    /// <summary>Gets кількість одиниць товару в пункті видачі.</summary>
    public int Stock { get; init; }
}
