using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;
using PickupPoint.Core.Errors;

namespace PickupPoint.Core.Services;

/// <summary>
/// Прикладний сервіс замовлень: приймає замовлення, сповіщає одержувача
/// та рахує суму за підставленою політикою.
/// </summary>
public sealed class PickupOrderService
{
    private readonly IPickupOrderRepository _repository;
    private readonly IPricingPolicy _pricing;
    private readonly INotifier _notifier;

    /// <summary>
    /// Створює сервіс із зовнішніми залежностями.
    /// </summary>
    /// <param name="repository">Сховище замовлень.</param>
    /// <param name="pricing">Політика ціноутворення.</param>
    /// <param name="notifier">Сповіщувач одержувача.</param>
    /// <exception cref="ArgumentNullException">
    /// Будь-яка залежність — null.
    /// </exception>
    public PickupOrderService(
        IPickupOrderRepository repository,
        IPricingPolicy pricing,
        INotifier notifier)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));
        _pricing = pricing
            ?? throw new ArgumentNullException(nameof(pricing));
        _notifier = notifier
            ?? throw new ArgumentNullException(nameof(notifier));
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
        _repository.Add(order);
        _notifier.Notify(
            email,
            $"Замовлення № {order.Id}",
            "Замовлення прийнято в пункті видачі.");
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
        PickupOrder order = RequireOrder(orderId);
        if (order.Status != PickupStatus.New)
        {
            throw new DomainRuleException(
                "order.editable",
                $"Стан {order.Status}: замовлення {orderId} не редагується.");
        }

        order.AddLine(line);
    }

    /// <summary>
    /// Тимчасовий стан «до власного винятку»: відсутність замовлення
    /// позначають значенням <c>null</c>.
    /// </summary>
    /// <param name="orderId">Ідентифікатор замовлення.</param>
    /// <returns>Замовлення або <c>null</c>, якщо його не знайдено.</returns>
    public PickupOrder? Load(int orderId) => _repository.GetById(orderId);

    /// <summary>
    /// Тимчасовий стан «до власного винятку»: результат операції скасування
    /// повертають числовим кодом.
    /// </summary>
    /// <param name="orderId">Ідентифікатор замовлення.</param>
    /// <param name="reason">Причина скасування.</param>
    /// <returns>0 — успіх; -1 — немає замовлення; -2 — стан забороняє
    /// скасування; -3 — причина порожня.</returns>
    public int CancelByCode(int orderId, string reason)
    {
        PickupOrder? order = _repository.GetById(orderId);
        if (order is null)
        {
            return -1;
        }

        if (order.Status is not (PickupStatus.New or PickupStatus.Arrived))
        {
            return -2;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return -3;
        }

        order.Cancel();
        return 0;
    }

    /// <summary>Обчислює суму замовлення за підставленою політикою.</summary>
    /// <param name="order">Замовлення, суму якого обчислюють.</param>
    /// <returns>Сума позицій у гривнях, округлена до копійок.</returns>
    /// <exception cref="ArgumentNullException">Замовлення — null.</exception>
    public decimal TotalOf(PickupOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
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
    private PickupOrder RequireOrder(int orderId) =>
        _repository.GetById(orderId)
        ?? throw new DomainRuleException(
            "order.exists",
            $"Замовлення {orderId} не знайдено");
}
