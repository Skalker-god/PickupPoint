using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;
using PickupPoint.Core.Errors;
using PickupPoint.Core.Pricing;
using PickupPoint.Core.Reporting;
using PickupPoint.Core.Services;
using PickupPoint.Core.Storage;

namespace PickupPoint.App;

[SuppressMessage(
    "Performance",
    "CA1848:Use the LoggerMessage delegates",
    Justification = "Верхній обробник ЛР 5 демонструє прямі виклики ILogger.")]
internal static class Program
{
    private static int Main()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.TimestampFormat = "HH:mm:ss ";
                options.IncludeScopes = true;
            });
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        ILogger<PickupOrderService> logger =
            factory.CreateLogger<PickupOrderService>();
        IPickupOrderRepository repository = new InMemoryPickupOrderRepository();
        IPricingPolicy pricing = new DiscountPricingPolicy(0.05m);
        INotifier notifier = new ConsoleNotifier();
        PickupOrderService service = new(repository, pricing, notifier, logger);
        try
        {
            Run(service, repository, pricing);
            return 0;
        }
        catch (DomainRuleException ex)
        {
            logger.LogWarning(
                "Правило {Rule}: {Message}",
                ex.Rule,
                ex.Message);
            Console.WriteLine($"Помилка: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Непередбачений збій");
            Console.WriteLine("Внутрішня помилка. Дивіться журнал.");
            return 2;
        }
    }

    private static void Run(
        PickupOrderService service,
        IPickupOrderRepository repository,
        IPricingPolicy pricing)
    {
        PickupOrder order = new(1, 100, DateTimeOffset.Now);
        service.Place(order, "i@ex.com");
        Console.WriteLine("Демонстраційне замовлення № 1 створено.");
        Console.WriteLine(
            "Команди: 1 — додати позицію, 2 — прибуло, 3 — видати, "
            + "4 — скасувати, 5 — звіт, 0 — вихід.");
        bool running = true;
        while (running)
        {
            Console.Write("Команда: ");
            string? raw = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(raw))
            {
                Console.WriteLine("Завершення роботи.");
                return;
            }

            switch (raw.Trim())
            {
                case "0":
                    running = false;
                    break;
                case "1":
                    AddLineInteractive(service, order);
                    break;
                case "2":
                    Execute(
                        order.MarkArrived,
                        "Замовлення прибуло до пункту видачі.");
                    break;
                case "3":
                    Execute(
                        () => order.Issue(DateTimeOffset.Now),
                        "Замовлення видано одержувачу.");
                    break;
                case "4":
                    CancelInteractive(service, order);
                    break;
                case "5":
                    ShowReport(service, repository, pricing, order);
                    break;
                default:
                    Console.WriteLine("Невідома команда, спробуйте ще раз.");
                    break;
            }
        }
    }

    private static void AddLineInteractive(
        PickupOrderService service,
        PickupOrder order)
    {
        Console.Write("Артикул (порожній — назад): ");
        string? sku = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(sku))
        {
            return;
        }

        while (true)
        {
            Console.Write("Кількість: ");
            string? quantity = Console.ReadLine() ?? string.Empty;
            Console.Write("Ціна (десятковий роздільник — крапка): ");
            string? price = Console.ReadLine() ?? string.Empty;
            Result<PickupOrderLine> parsed =
                PickupOrderLineParser.Parse(sku, quantity, price);
            if (!parsed.IsSuccess)
            {
                Console.WriteLine($"Помилка вводу: {parsed.Error}");
                continue;
            }

            try
            {
                service.AddLine(order.Id, parsed.Value!);
                Console.WriteLine("Позицію додано.");
                return;
            }
            catch (DomainRuleException ex)
            {
                Console.WriteLine($"Помилка правила {ex.Rule}: {ex.Message}");
                return;
            }
        }
    }

    private static void CancelInteractive(
        PickupOrderService service,
        PickupOrder order)
    {
        Console.Write("Причина скасування: ");
        string? reason = Console.ReadLine() ?? string.Empty;
        try
        {
            service.Cancel(order.Id, reason);
            Console.WriteLine("Замовлення скасовано.");
        }
        catch (DomainRuleException ex)
        {
            Console.WriteLine($"Помилка правила {ex.Rule}: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Помилка вводу: {ex.Message}");
        }
    }

    private static void ShowReport(
        PickupOrderService service,
        IPickupOrderRepository repository,
        IPricingPolicy pricing,
        PickupOrder order)
    {
        PickupOrderCsvReport report = new(pricing);
        Console.WriteLine(report.BuildCsv(repository.GetAll()).TrimEnd());
        decimal total = service.TotalOf(order);
        Console.WriteLine(
            "Сума: " + total.ToString("0.00", CultureInfo.InvariantCulture));
    }

    private static void Execute(Action action, string successMessage)
    {
        try
        {
            action();
            Console.WriteLine(successMessage);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Неможливо: {ex.Message}");
        }
    }
}
