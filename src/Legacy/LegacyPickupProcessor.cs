using System;
using System.Collections.Generic;

namespace PickupPoint.Legacy;

public class Recipient
{
    public int Id;
    public string Name = "";
    public string Email = "";
    public string Kind = "regular";
    public int DoneCount;
    public DateTime SinceUtc;
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

    public string Handle(int orderId, int recipientId,
        string recipientName, string? recipientMail,
        string recipientKind, int recipientDone,
        List<PickupLine>? items, string state,
        string currency, DateTime createdAt, bool sendMail)
    {
        if (items != null)
        {
            if (items.Count > 0)
            {
                if (state == "new" || state == "paid")
                {
                    if (recipientMail != null &&
                        recipientMail.Contains("@"))
                    {
                        _log.Add("ok " + orderId);
                    }
                    else
                    {
                        return "ERR: mail";
                    }
                }
                else
                {
                    return "ERR: state";
                }
            }
            else
            {
                return "ERR: empty";
            }
        }
        else
        {
            return "ERR: null";
        }

        _tmpSum = 0m;
        for (int i = 0; i < items.Count; i++)
        {
            _tmpSum += items[i].Qty * items[i].Price;
        }

        _tmpDiscount = 0m;
        if (recipientKind == "vip")
        {
            _tmpDiscount = _tmpSum * 0.15m;
            if (_tmpDiscount > 500m) _tmpDiscount = 500m;
        }
        else if (recipientKind == "staff")
        {
            _tmpDiscount = _tmpSum * 0.30m;
            if (_tmpDiscount > 500m) _tmpDiscount = 500m;
        }
        else if (recipientDone > 10)
        {
            _tmpDiscount = _tmpSum * 0.05m;
            if (_tmpDiscount > 500m) _tmpDiscount = 500m;
        }

        decimal ship = 0m;
        if (_tmpSum - _tmpDiscount < 1000m) ship = 60m;
        decimal total = _tmpSum - _tmpDiscount + ship;

        string txt = "Документ #" + orderId + "\n";
        txt += "Клієнт: " + recipientName + "\n";
        for (int i = 0; i < items.Count; i++)
        {
            txt += items[i].Code + " x" + items[i].Qty + " = " +
                (items[i].Qty * items[i].Price)
                .ToString("0.00") + " " + currency + "\n";
        }

        txt += "Знижка: " + _tmpDiscount.ToString("0.00") +
            " " + currency + "\n";
        txt += "Доставка: " + ship.ToString("0.00") +
            " " + currency + "\n";
        txt += "Разом: " + total.ToString("0.00") +
            " " + currency + "\n";
        if (sendMail)
        {
            _log.Add("mail -> " + recipientMail);
        }

        return txt;
    }

    public decimal Preview(string recipientKind, int recipientDone,
        List<PickupLine> items)
    {
        decimal s = 0m;
        foreach (var it in items)
        {
            s += it.Qty * it.Price;
        }

        decimal d = 0m;
        if (recipientKind == "vip")
        {
            d = s * 0.15m;
            if (d > 500m) d = 500m;
        }
        else if (recipientKind == "staff")
        {
            d = s * 0.30m;
            if (d > 500m) d = 500m;
        }
        else if (recipientDone > 10)
        {
            d = s * 0.05m;
            if (d > 500m) d = 500m;
        }

        decimal sh = 0m;
        if (s - d < 1000m) sh = 60m;
        return s - d + sh;
    }

    public string DescribeClient(Recipient c)
    {
        string s = c.Name.Trim().ToUpper();
        if (c.Kind == "vip") s += " [VIP]";
        if (c.DoneCount > 10) s += " [ЛОЯЛЬНИЙ]";
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
