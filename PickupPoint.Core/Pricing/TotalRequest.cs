using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Pricing;

/// <summary>Вхідні дані розрахунку підсумку замовлення.</summary>
/// <param name="OrderId">Номер замовлення.</param>
/// <param name="Recipient">Дані одержувача.</param>
/// <param name="Lines">Позиції замовлення.</param>
/// <param name="CreatedOn">День створення замовлення.</param>
/// <param name="CouponCode">Код купона або null.</param>
/// <param name="Status">Поточний стан замовлення.</param>
/// <param name="DeliveryPrice">Тариф доставки, грн.</param>
public sealed record TotalRequest(
    int OrderId,
    RecipientInfo Recipient,
    IReadOnlyList<PickupOrderLine> Lines,
    DateOnly CreatedOn,
    string? CouponCode,
    PickupStatus Status,
    decimal DeliveryPrice);
