using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Storage;

/// <summary>Сховище замовлень у пам’яті за ключем ідентифікатора.</summary>
public sealed class InMemoryPickupOrderRepository : IPickupOrderRepository
{
    private readonly Dictionary<int, PickupOrder> _orders = new();

    /// <summary>Додає замовлення; дублікат ідентифікатора заборонений.</summary>
    /// <param name="order">Замовлення, яке додають.</param>
    /// <exception cref="ArgumentNullException">Замовлення — null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Замовлення з таким ідентифікатором уже є.
    /// </exception>
    public void Add(PickupOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (!_orders.TryAdd(order.Id, order))
        {
            throw new InvalidOperationException(
                $"Замовлення № {order.Id} уже є у сховищі.");
        }
    }

    /// <summary>Знаходить замовлення за ідентифікатором.</summary>
    /// <param name="id">Ідентифікатор замовлення.</param>
    /// <returns>Замовлення або null, якщо його немає.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Ідентифікатор не додатний.
    /// </exception>
    public PickupOrder? GetById(int id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        return _orders.TryGetValue(id, out PickupOrder? order) ? order : null;
    }

    /// <summary>Повертає копію списку всіх замовлень.</summary>
    /// <returns>Замовлення, збережені у сховищі.</returns>
    public IReadOnlyList<PickupOrder> GetAll() => _orders.Values.ToList();
}
