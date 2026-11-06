using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Pricing;

/// <summary>Політика зі знижкою на задану частку.</summary>
public sealed class DiscountPricingPolicy : IPricingPolicy
{
    private const decimal MaxRate = 0.5m;

    private readonly decimal _rate;

    /// <summary>Створює політику зі знижкою.</summary>
    /// <param name="rate">Частка знижки від 0 до 0,5.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Частка поза діапазоном від 0 до 0,5.
    /// </exception>
    public DiscountPricingPolicy(decimal rate)
    {
        if (rate < 0m || rate > MaxRate)
        {
            throw new ArgumentOutOfRangeException(nameof(rate));
        }

        _rate = rate;
    }

    /// <summary>Обчислює ціну позиції зі знижкою.</summary>
    /// <param name="line">Позиція замовлення.</param>
    /// <returns>Ціна позиції, округлена до копійок.</returns>
    /// <exception cref="ArgumentNullException">Позиція — null.</exception>
    public decimal PriceOf(PickupOrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return Math.Round(
            line.Amount * (1m - _rate),
            2,
            MidpointRounding.AwayFromZero);
    }
}
