namespace PickupPoint.Core.Domain;

/// <summary>Позиція замовлення: товар, кількість і ціна за одиницю.</summary>
public sealed class PickupOrderLine
{
    /// <summary>Артикул товару в позиції.</summary>
    public string Sku { get; init; } = string.Empty;

    /// <summary>Кількість одиниць товару.</summary>
    public int Quantity { get; init; }

    /// <summary>Ціна за одиницю товару, грн.</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>Сума позиції: кількість, помножена на ціну за одиницю.</summary>
    public decimal Amount => Quantity * UnitPrice;
}
