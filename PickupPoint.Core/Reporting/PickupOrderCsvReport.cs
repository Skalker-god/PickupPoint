using System.Globalization;
using System.Text;
using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Reporting;

/// <summary>
/// Формує CSV-звіт про замовлення; нічого не пише на диск і не друкує.
/// </summary>
public sealed class PickupOrderCsvReport
{
    private readonly IPricingPolicy _pricing;

    /// <summary>Створює форматувальник із політикою для підсумків.</summary>
    /// <param name="pricing">Політика ціноутворення.</param>
    /// <exception cref="ArgumentNullException">Політика — null.</exception>
    public PickupOrderCsvReport(IPricingPolicy pricing)
    {
        _pricing = pricing
            ?? throw new ArgumentNullException(nameof(pricing));
    }

    /// <summary>Будує CSV-текст для переліку замовлень.</summary>
    /// <param name="orders">Замовлення, які потрапляють у звіт.</param>
    /// <returns>Багаторядковий CSV-текст.</returns>
    /// <exception cref="ArgumentNullException">Список — null.</exception>
    public string BuildCsv(IReadOnlyList<PickupOrder> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);
        StringBuilder text = new();
        text.AppendLine("id;recipient;status;total");
        foreach (PickupOrder order in orders)
        {
            decimal total = 0m;
            foreach (PickupOrderLine line in order.Lines)
            {
                total += _pricing.PriceOf(line);
            }

            text.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0};{1};{2};{3:F2}",
                order.Id,
                order.RecipientId,
                order.Status,
                total));
        }

        return text.ToString();
    }
}
