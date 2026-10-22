namespace PickupPoint.Core.Domain;
/// <summary>Замовлення одержувача, яке видається в пункті видачі.</summary>
public sealed class PickupOrder
{
    private readonly List<PickupOrderLine> _lines = new();
    public int Id { get; init; }
    public int RecipientId { get; init; }
    public PickupStatus Status { get; set; } = PickupStatus.New;
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<PickupOrderLine> Lines => _lines;

    /// <summary>Додає позицію до замовлення.</summary>
    public void AddLine(PickupOrderLine line) => _lines.Add(line);

    /// <summary>Обчислює суму замовлення.</summary>
    public decimal Total()
    {
        decimal sum = 0m;
        foreach (PickupOrderLine line in _lines)
        {
            sum += line.Amount;
        }

        return sum;
    }
}