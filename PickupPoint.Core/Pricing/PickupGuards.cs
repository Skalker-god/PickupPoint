using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Pricing;

internal static class PickupGuards
{
    public static void EnsureHeaderValid(
        int orderId, string recipientName, string recipientEmail)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(orderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);
        if (!recipientEmail.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Пошта має містити символ @.",
                nameof(recipientEmail));
        }
    }

    public static void EnsureLineValid(PickupOrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentException.ThrowIfNullOrWhiteSpace(line.Sku);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            line.Quantity, nameof(line.Quantity));
        ArgumentOutOfRangeException.ThrowIfNegative(
            line.UnitPrice, nameof(line.UnitPrice));
    }

    public static void EnsureLinesValid(
        IReadOnlyList<PickupOrderLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0)
        {
            throw new ArgumentException(
                "Замовлення не містить жодної позиції.",
                nameof(lines));
        }

        foreach (PickupOrderLine line in lines)
        {
            EnsureLineValid(line);
        }
    }

    public static void EnsureRequestValid(TotalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Recipient);
        EnsureHeaderValid(
            request.OrderId,
            request.Recipient.Name,
            request.Recipient.Email);
        EnsureLinesValid(request.Lines);
        ArgumentOutOfRangeException.ThrowIfNegative(
            request.DeliveryPrice);
    }
}
