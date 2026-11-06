using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;

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
}
