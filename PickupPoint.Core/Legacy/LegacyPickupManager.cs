namespace PickupPoint.Core.Legacy;

public class LegacyPickupManager
{
    private readonly LegacyPickupStore _store = new LegacyPickupStore();
    private readonly LegacyMailer _mailer = new LegacyMailer();

    public decimal Place(LegacyPickupOrder order, string customerType)
    {
        decimal total = 0m;
        foreach (LegacyPickupLine line in order.Lines)
        {
            total += line.UnitPrice * line.Quantity;
        }

        if (customerType == "regular")
        {
            total *= 0.95m;
        }
        else if (customerType == "vip")
        {
            total *= 0.90m;
        }
        else if (customerType == "staff")
        {
            total *= 0.70m;
        }

        if (total > 1000m)
        {
            total -= 50m;
        }

        order.Total = total;
        order.Status = 1;
        _store.SaveToFile(order, "orders.json");
        _mailer.Send(order.Email, "Замовлення " + order.Id, string.Empty + total);
        return total;
    }

    public string BuildCsvReport(List<LegacyPickupOrder> orders)
    {
        string text = "id;total;status" + Environment.NewLine;
        foreach (LegacyPickupOrder order in orders)
        {
            text += order.Id + ";" + order.Total + ";" + order.Status;
        }

        File.WriteAllText("report.csv", text);
        Console.WriteLine("Звіт збережено");
        return text;
    }

    public void Archive(LegacyPickupOrder order)
    {
        LegacyPickupOrder archived = new ArchivedPickupOrder(order.Id);
        foreach (LegacyPickupLine line in order.Lines)
        {
            archived.AddLine(line);
        }

        _store.SaveToFile(archived, "archive.json");
    }
}
