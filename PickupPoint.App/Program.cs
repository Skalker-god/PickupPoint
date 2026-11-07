using System.Globalization;
using PickupPoint.App;
using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;
using PickupPoint.Core.Errors;
using PickupPoint.Core.Pricing;
using PickupPoint.Core.Reporting;
using PickupPoint.Core.Services;
using PickupPoint.Core.Storage;

// Композиційний корінь: єдине місце, де створюють конкретні реалізації.
#pragma warning disable CA1859 // залежність навмисно тримаємо як абстракцію
IPickupOrderRepository repository = new InMemoryPickupOrderRepository();
#pragma warning restore CA1859
IPricingPolicy pricing = new DiscountPricingPolicy(0.05m);
INotifier notifier = new ConsoleNotifier();
PickupOrderService service = new(repository, pricing, notifier);

PickupOrder order = new(1, 100, DateTimeOffset.Now);
service.Place(order, "i@ex.com");
Result<PickupOrderLine> parsed =
    PickupOrderLineParser.Parse("SKU-1", "2", "250.00");
if (!parsed.IsSuccess)
{
    Console.WriteLine($"Помилка вводу: {parsed.Error}");
    return 1;
}

service.AddLine(order.Id, parsed.Value!);

decimal total = service.TotalOf(order);
Console.WriteLine(
    "Сума: " + total.ToString("0.00", CultureInfo.InvariantCulture));

PickupOrderCsvReport report = new(pricing);
Console.WriteLine(report.BuildCsv(repository.GetAll()).TrimEnd());
return 0;
