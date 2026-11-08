using System.Text;

namespace PickupPoint.Legacy;

public sealed class ReportBuilder
{
    public string Build(
        OrderRequest request,
        decimal discount,
        decimal shipping,
        decimal total)
    {
        var sb = new StringBuilder();
        sb.Append("Документ #").Append(request.OrderId).Append('\n');
        sb.Append("Клієнт: ").Append(request.Recipient.Name).Append('\n');
        foreach (var it in request.Items!)
        {
            sb.Append(it.Code).Append(" x").Append(it.Qty).Append(" = ")
                .Append((it.Qty * it.Price).ToString("0.00"))
                .Append(' ').Append(request.Currency).Append('\n');
        }

        sb.Append("Знижка: ").Append(discount.ToString("0.00"))
            .Append(' ').Append(request.Currency).Append('\n');
        sb.Append("Доставка: ").Append(shipping.ToString("0.00"))
            .Append(' ').Append(request.Currency).Append('\n');
        sb.Append("Разом: ").Append(total.ToString("0.00"))
            .Append(' ').Append(request.Currency).Append('\n');
        return sb.ToString();
    }
}
