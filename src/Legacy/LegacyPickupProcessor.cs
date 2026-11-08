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

    public string Describe()
    {
        string s = Name.Trim().ToUpper();
        if (Kind == RecipientKind.Vip) s += " [VIP]";
        if (IsLoyal) s += " [ЛОЯЛЬНИЙ]";
        s += " <" + Email.ToLower() + ">";
        int years = DateTime.Now.Year - SinceUtc.Year;
        s += " стаж " + years;
        return s;
    }
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
    private readonly List<string> _log = new();
    private readonly DiscountPolicy _discounts = new();
    private readonly ShippingPolicy _shipping = new();
    private readonly ReportBuilder _report = new();

    public string Handle(OrderRequest request)
    {
        string? error = Validate(request);
        if (error != null) return error;
        _log.Add("ok " + request.OrderId);

        decimal subtotal = Subtotal(request.Items!);
        decimal discount = _discounts.For(subtotal, request.Recipient);
        decimal ship = _shipping.For(subtotal - discount);
        decimal total = subtotal - discount + ship;

        if (request.SendMail)
        {
            _log.Add("mail -> " + request.Recipient.Email);
        }

        return _report.Build(request, discount, ship, total);
    }

    private static string? Validate(OrderRequest request)
    {
        if (request.Items is null) return "ERR: null";
        if (request.Items.Count == 0) return "ERR: empty";
        if (request.State is not (OrderState.New or OrderState.Paid))
            return "ERR: state";
        if (!request.Recipient.HasValidEmail) return "ERR: mail";
        return null;
    }

    private static decimal Subtotal(IReadOnlyList<PickupLine> items)
    {
        decimal sum = 0m;
        foreach (var it in items)
        {
            sum += it.Qty * it.Price;
        }

        return sum;
    }

    public decimal Preview(Recipient recipient,
        IReadOnlyList<PickupLine> items)
    {
        decimal subtotal = Subtotal(items);
        decimal discount = _discounts.For(subtotal, recipient);
        decimal payable = subtotal - discount;
        return payable + _shipping.For(payable);
    }

    public string DumpLog()
    {
        string r = "";
        foreach (var l in _log) r += l + "\n";
        return r;
    }
}
