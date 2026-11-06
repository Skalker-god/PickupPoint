namespace PickupPoint.Core.Domain;

/// <summary>
/// Архівний документ: містить замовлення всередині й показує лише його дані.
/// </summary>
public sealed class ArchivedPickupOrder
{
    private readonly PickupOrder _order;

    /// <summary>Створює архівний запис над наявним замовленням.</summary>
    /// <param name="order">Замовлення, яке архівують.</param>
    /// <param name="archivedAt">Дата архівування.</param>
    /// <exception cref="ArgumentNullException">Замовлення — null.</exception>
    public ArchivedPickupOrder(PickupOrder order, DateOnly archivedAt)
    {
        ArgumentNullException.ThrowIfNull(order);
        _order = order;
        ArchivedAt = archivedAt;
    }

    /// <summary>Дата архівування замовлення.</summary>
    public DateOnly ArchivedAt { get; }

    /// <summary>Ідентифікатор замовлення.</summary>
    public int Id => _order.Id;

    /// <summary>Стан замовлення на момент архівування.</summary>
    public PickupStatus Status => _order.Status;

    /// <summary>Позиції замовлення, доступні лише для читання.</summary>
    public IReadOnlyList<PickupOrderLine> Lines => _order.Lines;
}
