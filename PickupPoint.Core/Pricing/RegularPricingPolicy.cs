using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Pricing;

/// <summary>Базова політика: ціна позиції без знижки.</summary>
public sealed class RegularPricingPolicy : IPricingPolicy
{
    /// <summary>Обчислює ціну позиції без знижки.</summary>
    /// <param name="line">Позиція замовлення.</param>
    /// <returns>Сума позиції, округлена до копійок.</returns>
    /// <exception cref="ArgumentNullException">Позиція — null.</exception>
    public decimal PriceOf(PickupOrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return Math.Round(
            line.Amount,
            2,
            MidpointRounding.AwayFromZero);
    }
}
