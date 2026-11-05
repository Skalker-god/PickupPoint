using System.Globalization;
using System.Text;
using PickupPoint.Core.Domain;
using PickupPoint.Core.Pricing;

namespace PickupPoint.Core.Reporting;

/// <summary>Формування тексту звіту про замовлення.</summary>
public static class OrderReport
{
    /// <summary>Будує текст звіту. Чиста функція: нічого не друкує.</summary>
    /// <param name="request">Запит, за яким виконано розрахунок.</param>
    /// <param name="result">Результат розрахунку.</param>
    /// <returns>Багаторядковий текст звіту.</returns>
    /// <exception cref="ArgumentNullException">Запит або результат — null.</exception>
    public static string TextOf(TotalRequest request, TotalResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder text = new();
        text.AppendLine($"Замовлення № {request.OrderId}");
        text.AppendLine($"Одержувач: {request.Recipient.Name}");
        foreach (PickupOrderLine line in request.Lines)
        {
            text.AppendLine($"{line.Sku} x {line.Quantity}");
        }

        text.AppendLine(string.Format(
            culture, "Разом: {0:F2}", result.Total));
        text.AppendLine($"Стан: {result.Status}");
        return text.ToString();
    }
}
