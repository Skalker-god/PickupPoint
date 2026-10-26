namespace PickupPoint.Core.Domain;

/// <summary>Замовлення одержувача, яке видається в пункті видачі.</summary>
public sealed class PickupOrder
{
    private readonly List<PickupOrderLine> _lines = new();

    /// <summary>Gets унікальний ідентифікатор замовлення.</summary>
    public int Id { get; init; }

    /// <summary>Gets ідентифікатор одержувача замовлення.</summary>
    public int RecipientId { get; init; }

    /// <summary>Gets or sets поточний стан замовлення.</summary>
    public PickupStatus Status { get; set; } = PickupStatus.New;

    /// <summary>Gets дата й час створення замовлення.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Gets позиції замовлення, доступні лише для читання.</summary>
    public IReadOnlyList<PickupOrderLine> Lines => _lines;

    /// <summary>Додає позицію до замовлення.</summary>
    /// <param name="line">Позиція, яку додають до замовлення.</param>
    public void AddLine(PickupOrderLine line) => _lines.Add(line);

    /// <summary>Обчислює суму замовлення.</summary>
    /// <returns>Сума всіх позицій замовлення.</returns>
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
