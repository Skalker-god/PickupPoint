namespace PickupPoint.Core.Domain;

/// <summary>Замовлення одержувача, яке видається в пункті видачі.</summary>
public sealed class PickupOrder
{
    private readonly List<PickupOrderLine> _lines = new();

    /// <summary>Унікальний ідентифікатор замовлення.</summary>
    public int Id { get; init; }

    /// <summary>Ідентифікатор одержувача замовлення.</summary>
    public int RecipientId { get; init; }

    /// <summary>Поточний стан замовлення.</summary>
    public PickupStatus Status { get; set; } = PickupStatus.New;

    /// <summary>Дата й час створення замовлення.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Позиції замовлення, доступні лише для читання.</summary>
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
