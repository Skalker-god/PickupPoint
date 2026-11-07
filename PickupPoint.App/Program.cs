using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Audit;
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
            builder.SetMinimumLevel(ReadMinimumLevel());
        });
        ILogger<PickupOrderService> logger =
            factory.CreateLogger<PickupOrderService>();
        IPickupOrderRepository repository = new InMemoryPickupOrderRepository();
        IPricingPolicy pricing = new DiscountPricingPolicy(0.05m);
        INotifier notifier = new ConsoleNotifier();
        PickupOrderService service = new(repository, pricing, notifier, logger);
        try
        {
            using FileAuditLog auditLog = new("audit.log");
            Run(service, repository, pricing, auditLog);
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
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, "Немає прав на запис файлу звіту");
            Console.WriteLine(
                "Не вдалося зберегти звіт: немає прав на запис.");
            return 3;
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Збій файлового ресурсу");
            Console.WriteLine(
                "Не вдалося виконати файлову операцію: журнал або звіт.");
            return 3;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Непередбачений збій");
            Console.WriteLine("Внутрішня помилка. Дивіться журнал.");
            return 2;
        }
    }

    private static LogLevel ReadMinimumLevel()
    {
        string? raw = Environment.GetEnvironmentVariable(
            "PICKUPPOINT_LOG_LEVEL");
        return Enum.TryParse(raw, ignoreCase: true, out LogLevel level)
            ? level
            : LogLevel.Debug;
    }

    private static void Run(
        PickupOrderService service,
        IPickupOrderRepository repository,
        IPricingPolicy pricing,
        FileAuditLog auditLog)
    {
        PickupOrder order = new(1, 100, DateTimeOffset.Now);
        service.Place(order, "i@ex.com");
        auditLog.Write("place", order.Id);
        Console.WriteLine("Демонстраційне замовлення № 1 створено.");
        Console.WriteLine(
            "1-додати 2-прибуло 3-видати 4-скасувати "
            + "5-звіт 6-експорт 0-вихід");
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
                    AddLineInteractive(service, order, auditLog);
                    break;
                case "2":
                    Execute(
                        () =>
                        {
                            order.MarkArrived();
                            auditLog.Write("arrive", order.Id);
                        },
                        "Замовлення прибуло до пункту видачі.");
                    break;
                case "3":
                    Execute(
                        () =>
                        {
                            order.Issue(DateTimeOffset.Now);
                            auditLog.Write("issue", order.Id);
                        },
                        "Замовлення видано одержувачу.");
                    break;
                case "4":
                    CancelInteractive(service, order, auditLog);
                    break;
                case "5":
                    ShowReport(service, repository, pricing, order, auditLog);
                    break;
                case "6":
                    ExportReport(repository, pricing, order, auditLog);
                    break;
                default:
                    Console.WriteLine("Невідома команда, спробуйте ще раз.");
                    break;
            }
        }
    }

    private static void AddLineInteractive(
        PickupOrderService service,
        PickupOrder order,
        FileAuditLog auditLog)
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
                auditLog.Write("add-line", order.Id);
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
        PickupOrder order,
        FileAuditLog auditLog)
    {
        Console.Write("Причина скасування: ");
        string? reason = Console.ReadLine() ?? string.Empty;
        try
        {
            service.Cancel(order.Id, reason);
            auditLog.Write("cancel", order.Id);
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
        PickupOrder order,
        FileAuditLog auditLog)
    {
        PickupOrderCsvReport report = new(pricing);
        Console.WriteLine(report.BuildCsv(repository.GetAll()).TrimEnd());
        decimal total = service.TotalOf(order);
        Console.WriteLine(
            "Сума: " + total.ToString("0.00", CultureInfo.InvariantCulture));
        auditLog.Write("report", order.Id);
    }

    private static void ExportReport(
        IPickupOrderRepository repository,
        IPricingPolicy pricing,
        PickupOrder order,
        FileAuditLog auditLog)
    {
        const string ReportPath = "report.csv";
        PickupOrderCsvReport report = new(pricing);
        string csv = report.BuildCsv(repository.GetAll());
        ReportFileWriter.Export(ReportPath, csv);
        auditLog.Write("export", order.Id);
        Console.WriteLine(
            $"Звіт збережено у файлі {ReportPath}");
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
