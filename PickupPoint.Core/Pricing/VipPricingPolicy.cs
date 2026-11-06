using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Pricing;

/// <summary>Політика для постійного одержувача зі знижкою 10 %.</summary>
public sealed class VipPricingPolicy : IPricingPolicy
{
    private const decimal VipRate = 0.10m;

    /// <summary>Обчислює ціну позиції для постійного одержувача.</summary>
    /// <param name="line">Позиція замовлення.</param>
    /// <returns>Ціна позиції, округлена до копійок.</returns>
    /// <exception cref="ArgumentNullException">Позиція — null.</exception>
    public decimal PriceOf(PickupOrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return Math.Round(
            line.Amount * (1m - VipRate),
            2,
            MidpointRounding.AwayFromZero);
    }
}
