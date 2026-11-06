namespace PickupPoint.Core.Domain;

/// <summary>
/// Незмінна позиція замовлення: артикул, кількість і ціна за одиницю.
/// </summary>
public sealed record PickupOrderLine
{
    /// <summary>
    /// Створює позицію та перевіряє її інваріант.
    /// </summary>
    /// <param name="sku">Артикул товару.</param>
    /// <param name="quantity">Кількість одиниць, більша за нуль.</param>
    /// <param name="unitPrice">Ціна за одиницю, невід’ємна.</param>
    /// <exception cref="ArgumentException">Артикул порожній.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Кількість не додатна або ціна від’ємна.
    /// </exception>
    public PickupOrderLine(string sku, int quantity, decimal unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>Артикул товару в позиції.</summary>
    public string Sku { get; }

    /// <summary>Кількість одиниць товару.</summary>
    public int Quantity { get; }

    /// <summary>Ціна за одиницю товару, грн.</summary>
    public decimal UnitPrice { get; }

    /// <summary>Сума позиції: кількість, помножена на ціну за одиницю.</summary>
    public decimal Amount => Quantity * UnitPrice;
}
