namespace PickupPoint.Core.Domain;
/// <summary>Позиція замовлення: товар, кількість і ціна за одиницю.</summary>
public sealed class PickupOrderLine
{
    public string Sku { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Amount => Quantity * UnitPrice;
}
