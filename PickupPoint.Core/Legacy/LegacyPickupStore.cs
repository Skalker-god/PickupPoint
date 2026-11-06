namespace PickupPoint.Core.Legacy;

public sealed class LegacyPickupStore : ILegacyPickupStore
{
    private readonly Dictionary<int, LegacyPickupOrder> _orders = new();

    public void Add(LegacyPickupOrder order) => _orders[order.Id] = order;

    public LegacyPickupOrder GetById(int id) => _orders[id];

    public void SaveToFile(LegacyPickupOrder order, string path)
    {
        string text = order.Id + ";" + order.Total + ";" + order.Status;
        File.WriteAllText(path, text);
    }

    public void SendEmail(string to, string body)
    {
        Console.WriteLine("Лист для " + to + ": " + body);
    }

    public string ExportCsv()
    {
        return string.Join(
            Environment.NewLine,
            _orders.Values.Select(o => o.Id + ";" + o.Total + ";" + o.Status));
    }

    public void Backup(string folder)
    {
        foreach (LegacyPickupOrder order in _orders.Values)
        {
            File.WriteAllText(
                Path.Combine(folder, order.Id + ".json"),
                order.Total.ToString());
        }
    }

    public void PrintInvoice(LegacyPickupOrder order)
    {
        Console.WriteLine("Рахунок № " + order.Id + " на " + order.Total);
    }
}

public sealed class LegacyMailer
{
    public void Send(string to, string subject, string body)
    {
        Console.WriteLine(to + " | " + subject + " | " + body);
    }
}
