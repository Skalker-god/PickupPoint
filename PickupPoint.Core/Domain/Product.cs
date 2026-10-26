namespace PickupPoint.Core.Domain;

/// <summary>Товар із довідника пункту видачі.</summary>
public sealed class Product
{
    /// <summary>Артикул товару.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Назва товару.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Ціна за одиницю, грн.</summary>
    public decimal Price { get; init; }

    /// <summary>Кількість одиниць товару в пункті видачі.</summary>
    public int Stock { get; init; }
}
