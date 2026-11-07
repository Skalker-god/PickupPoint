using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;
using PickupPoint.Core.Errors;

namespace PickupPoint.Core.Services;

/// <summary>
/// Прикладний сервіс замовлень: приймає замовлення, сповіщає одержувача
/// та рахує суму за підставленою політикою.
/// </summary>
[SuppressMessage(
    "Performance",
    "CA1848:Use the LoggerMessage delegates",
    Justification = "Мета ЛР 5 — прямі виклики ILogger з іменованими заповнювачами.")]
public sealed class PickupOrderService
{
    private readonly IPickupOrderRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;
    private readonly ILogger<PickupOrderService> _log;

    /// <summary>
    /// Створює сервіс із зовнішніми залежностями.
    /// </summary>
    /// <param name="repository">Сховище замовлень.</param>
    /// <param name="pricing">Політика ціноутворення.</param>
    /// <param name="notifier">Сповіщувач одержувача.</param>
    /// <param name="logger">
    /// Структурований журнал; без нього використовують <see cref="NullLogger{T}"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Будь-яка обов’язкова залежність — null.
    /// </exception>
    public PickupOrderService(
        IPickupOrderRepository repository,
        IPricingPolicy pricing,
        INotifier notifier,
        ILogger<PickupOrderService>? logger = null)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing
            ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier
            ?? throw new ArgumentNullException(nameof(notifier));
        _log = logger ?? NullLogger<PickupOrderService>.Instance;
    }

    /// <summary>Приймає замовлення до пункту видачі й сповіщає одержувача.</summary>
    /// <param name="order">Замовлення, яке приймають.</param>
    /// <param name="email">Пошта одержувача для сповіщення.</param>
    /// <exception cref="ArgumentNullException">Замовлення — null.</exception>
    /// <exception cref="ArgumentException">Пошта порожня.</exception>
    public void Place(PickupOrder order, string email)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        using IDisposable? scope = _log.BeginScope("Order:{OrderId}", order.Id);
        _log.LogDebug("Реєстрація замовлення {OrderId}", order.Id);
        _repository.Add(order);
        _notifier.Notify(
            email,
            $"Замовлення № {order.Id}",
            "Замовлення прийнято в пункті видачі.");
        _log.LogInformation(
            "Замовлення {OrderId} прийнято, сповіщення для {Email}",
            order.Id,
            email);
    }

    /// <summary>
    /// Додає типізовану позицію до нового замовлення. Ядро не знає про рядки
    /// консолі: розбір виконано на барикаді в застосунку.
    /// </summary>
    /// <param name="orderId">Ідентифікатор замовлення.</param>
    /// <param name="line">Перевірена позиція замовлення.</param>
    /// <exception cref="ArgumentNullException">Позиція — null.</exception>
    /// <exception cref="DomainRuleException">
    /// Замовлення не знайдено або воно вже не у стані <see cref="PickupStatus.New"/>.
    /// </exception>
    public void AddLine(int orderId, PickupOrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        using IDisposable? scope = _log.BeginScope("Order:{OrderId}", orderId);
        _log.LogDebug(
            "Початок додавання позиції {Sku}, кількість {Quantity}",
            line.Sku,
            line.Quantity);
        PickupOrder order = RequireOrder(orderId);
        if (order.Lines.Any(existing => existing.Sku == line.Sku))
        {
            _log.LogWarning(
                "Позиція {Sku} уже є в замовленні {OrderId}",
                line.Sku,
                orderId);
        }

        try
        {
            order.AddLine(line);
        }
        catch (InvalidOperationException ex)
        {
            _log.LogError(
                ex,
                "Не вдалося додати позицію {Sku} до замовлення {OrderId}",
                line.Sku,
                orderId);
            throw new DomainRuleException(
                "order.editable",
                $"Стан {order.Status}: замовлення {orderId} не редагується.",
                ex);
        }

        _log.LogInformation(
            "Додано позицію {Sku} у кількості {Quantity} до замовлення {OrderId}",
            line.Sku,
            line.Quantity,
            orderId);
    }

    /// <summary>Знаходить замовлення або порушує правило його існування.</summary>
    /// <param name="orderId">Ідентифікатор замовлення.</param>
    /// <returns>Знайдене замовлення.</returns>
    /// <exception cref="DomainRuleException">Замовлення не знайдено.</exception>
    public PickupOrder Load(int orderId)
    {
        using IDisposable? scope = _log.BeginScope("Order:{OrderId}", orderId);
        _log.LogDebug("Пошук замовлення {OrderId}", orderId);
        PickupOrder order = RequireOrder(orderId);
        _log.LogInformation(
            "Замовлення {OrderId} знайдено, стан {Status}",
            orderId,
            order.Status);
        return order;
    }

    /// <summary>Скасовує замовлення, яке ще не видано.</summary>
    /// <param name="orderId">Ідентифікатор замовлення.</param>
    /// <param name="reason">Непорожня причина скасування.</param>
    /// <exception cref="ArgumentException">Причина порожня.</exception>
    /// <exception cref="DomainRuleException">
    /// Замовлення не знайдено або його стан забороняє скасування.
    /// </exception>
    public void Cancel(int orderId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        using IDisposable? scope = _log.BeginScope("Order:{OrderId}", orderId);
        _log.LogDebug(
            "Скасування замовлення {OrderId}, причина {Reason}",
            orderId,
            reason);
        PickupOrder order = RequireOrder(orderId);
        if (order.Status is not (PickupStatus.New or PickupStatus.Arrived))
        {
            _log.LogWarning(
                "Стан {Status} не дозволяє скасувати замовлення {OrderId}",
                order.Status,
                orderId);
            throw new DomainRuleException(
                "order.cancellable",
                $"Стан {order.Status}: замовлення {orderId} не скасовується.");
        }

        order.Cancel();
        _log.LogInformation(
            "Замовлення {OrderId} скасовано: {Reason}",
            orderId,
            reason);
    }

    /// <summary>Обчислює суму замовлення за підставленою політикою.</summary>
    /// <param name="order">Замовлення, суму якого обчислюють.</param>
    /// <returns>Сума позицій у гривнях, округлена до копійок.</returns>
    /// <exception cref="ArgumentNullException">Замовлення — null.</exception>
    public decimal TotalOf(PickupOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _log.LogDebug(
            "Розрахунок суми замовлення {OrderId}: позицій {LineCount}",
            order.Id,
            order.Lines.Count);
        decimal total = 0m;
        foreach (PickupOrderLine line in order.Lines)
        {
            total += _pricing.PriceOf(line);
        }

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Знаходить замовлення або порушує правило його існування.</summary>
    /// <param name="orderId">Ідентифікатор замовлення.</param>
    /// <returns>Знайдене замовлення.</returns>
    /// <exception cref="DomainRuleException">Замовлення не знайдено.</exception>
    private PickupOrder RequireOrder(int orderId)
    {
        PickupOrder? order = _repository.GetById(orderId);
        if (order is null)
        {
            _log.LogWarning(
                "Замовлення {OrderId} не знайдено",
                orderId);
            throw new DomainRuleException(
                "order.exists",
                $"Замовлення {orderId} не знайдено");
        }

        return order;
    }
}
