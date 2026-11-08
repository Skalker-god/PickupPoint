using System;
using System.Collections.Generic;

namespace PickupPoint.Legacy;

public enum RecipientKind
{
    Regular,
    Vip,
    Staff,
}

public enum OrderState
{
    Draft,
    New,
    Paid,
    Closed,
}

public sealed class Recipient
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public RecipientKind Kind { get; init; }
    public int DoneCount { get; init; }
    public DateTime SinceUtc { get; init; }
    public bool HasValidEmail => Email is not null && Email.Contains("@");
    public bool IsLoyal => DoneCount > PricingRules.LoyalOrders;
}

public class PickupLine
{
    public string Code = "";
    public int Qty;
    public decimal Price;
    public string Group = "";
}

public class LegacyPickupProcessor
{
    private decimal _tmpSum;
    private decimal _tmpDiscount;
    private readonly List<string> _log = new();
    private readonly DiscountPolicy _discounts = new();
    private readonly ShippingPolicy _shipping = new();
    private readonly ReportBuilder _report = new();

    public string Handle(int orderId, int recipientId,
        string recipientName, string? recipientMail,
        RecipientKind recipientKind, int recipientDone,
        List<PickupLine>? items, OrderState state,
        string currency, DateTime createdAt, bool sendMail)
    {
        if (items is null) return "ERR: null";
        if (items.Count == 0) return "ERR: empty";
        if (state is not (OrderState.New or OrderState.Paid))
            return "ERR: state";
        if (recipientMail is null || !recipientMail.Contains("@"))
            return "ERR: mail";
        _log.Add("ok " + orderId);

        _tmpSum = Subtotal(items);
        _tmpDiscount = _discounts.For(_tmpSum, recipientKind, recipientDone);
        decimal ship = _shipping.For(_tmpSum - _tmpDiscount);
        decimal total = _tmpSum - _tmpDiscount + ship;

        if (sendMail)
        {
            _log.Add("mail -> " + recipientMail);
        }

        return _report.Build(
            orderId,
            recipientName,
            items,
            _tmpDiscount,
            ship,
            total,
            currency);
    }

    private static decimal Subtotal(List<PickupLine> items)
    {
        decimal sum = 0m;
        foreach (var it in items)
        {
            sum += it.Qty * it.Price;
        }

        return sum;
    }

    public decimal Preview(RecipientKind recipientKind, int recipientDone,
        List<PickupLine> items)
    {
        decimal subtotal = Subtotal(items);
        decimal discount = _discounts.For(subtotal, recipientKind, recipientDone);
        decimal payable = subtotal - discount;
        return payable + _shipping.For(payable);
    }

    public string DescribeClient(Recipient c)
    {
        string s = c.Name.Trim().ToUpper();
        if (c.Kind == RecipientKind.Vip) s += " [VIP]";
        if (c.DoneCount > PricingRules.LoyalOrders) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + c.Email.ToLower() + ">";
        int years = DateTime.Now.Year - c.SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}
