namespace PickupPoint.Core.Domain;

/// <summary>Позиція замовлення: товар, кількість і ціна за одиницю.</summary>
public sealed class PickupOrderLine
{
    /// <summary>Gets артикул товару в позиції.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Gets кількість одиниць товару.</summary>
    public int Quantity { get; init; }

    /// <summary>Gets ціна за одиницю товару, грн.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>Gets сума позиції: кількість, помножена на ціну за одиницю.</summary>
    public decimal Amount => Quantity * UnitPrice;
}
