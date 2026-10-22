using System.Globalization;
using PickupPoint.Core.Domain;

PickupOrder order = new()
{
    Id = 1,
    RecipientId = 10,
    CreatedAt = DateTimeOffset.Now,
};

order.AddLine(new PickupOrderLine
{
    Sku = "PP-1001",
    Quantity = 3,
    UnitPrice = 59.90m,
});

order.AddLine(new PickupOrderLine
{
    Sku = "PP-2002",
    Quantity = 1,
    UnitPrice = 120.00m,
});

string total = order.Total()
    .ToString("F2", CultureInfo.InvariantCulture);

Console.WriteLine($"Замовлення #{order.Id}");
Console.WriteLine($"Стан: {order.Status}");
Console.WriteLine($"Позицій: {order.Lines.Count}");
Console.WriteLine($"Сума: {total}");