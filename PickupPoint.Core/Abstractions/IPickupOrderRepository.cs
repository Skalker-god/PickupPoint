using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Abstractions;

/// <summary>Сховище замовлень пункту видачі.</summary>
public interface IPickupOrderRepository
{
    /// <summary>Додає замовлення до сховища.</summary>
    /// <param name="order">Замовлення, яке додають.</param>
    void Add(PickupOrder order);

    /// <summary>Знаходить замовлення за ідентифікатором.</summary>
    /// <param name="id">Ідентифікатор замовлення.</param>
    /// <returns>Замовлення або null, якщо його немає.</returns>
    PickupOrder? GetById(int id);

    /// <summary>Повертає копію списку всіх замовлень.</summary>
    /// <returns>Замовлення, збережені у сховищі.</returns>
    IReadOnlyList<PickupOrder> GetAll();
}
