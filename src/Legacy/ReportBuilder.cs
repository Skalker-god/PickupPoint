using System.Text;

namespace PickupPoint.Legacy;

public sealed class ReportBuilder
{
    public string Build(
        int orderId,
        string recipientName,
        IReadOnlyList<PickupLine> items,
        decimal discount,
        decimal shipping,
        decimal total,
        string currency)
    {
        var sb = new StringBuilder();
        sb.Append("Документ #").Append(orderId).Append('\n');
        sb.Append("Клієнт: ").Append(recipientName).Append('\n');
        foreach (var it in items)
        {
            sb.Append(it.Code).Append(" x").Append(it.Qty).Append(" = ")
                .Append((it.Qty * it.Price).ToString("0.00"))
                .Append(' ').Append(currency).Append('\n');
        }

        sb.Append("Знижка: ").Append(discount.ToString("0.00"))
            .Append(' ').Append(currency).Append('\n');
        sb.Append("Доставка: ").Append(shipping.ToString("0.00"))
            .Append(' ').Append(currency).Append('\n');
        sb.Append("Разом: ").Append(total.ToString("0.00"))
            .Append(' ').Append(currency).Append('\n');
        return sb.ToString();
    }
}
