using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Abstractions;

/// <summary>Політика ціноутворення для однієї позиції замовлення.</summary>
public interface IPricingPolicy
{
    /// <summary>Обчислює ціну позиції за політикою.</summary>
    /// <param name="line">Позиція замовлення.</param>
    /// <returns>Ціна позиції в гривнях.</returns>
    decimal PriceOf(PickupOrderLine line);
}
