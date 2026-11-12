using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Pricing;

/// <summary>
/// Розрахунок підсумку замовлення: кожен крок розрахунку — окрема підпрограма.
/// </summary>
public static class OrderTotalCalculator
{
    /// <summary>
    /// Обчислює суму позицій із гуртовою знижкою. Не змінює стан.
    /// </summary>
    /// <param name="lines">Непорожній список коректних позицій.</param>
    /// <returns>Сума позицій у гривнях без округлення.</returns>
    /// <exception cref="ArgumentException">Список порожній.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Кількість чи ціна позиції некоректні.</exception>
    public static decimal SubtotalOf(IReadOnlyList<PickupOrderLine> lines)
    {
        PickupGuards.EnsureLinesValid(lines);
        decimal subtotal = 0m;
        foreach (PickupOrderLine line in lines)
        {
            decimal amount = line.Quantity * line.UnitPrice;
            subtotal += line.Quantity >= PricingRules.BulkQuantity
                ? amount * (1m - PricingRules.BulkRate)
                : amount;
        }

        return subtotal;
    }

    /// <summary>Обчислює знижку постійному одержувачу. Не змінює стан.</summary>
    /// <param name="amount">Сума, від якої рахується знижка, грн.</param>
    /// <param name="isRegular">Чи є одержувач постійним.</param>
    /// <returns>Розмір знижки в гривнях; 0 для звичайного одержувача.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Сума від’ємна.</exception>
    public static decimal RecipientDiscountOf(decimal amount, bool isRegular)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return isRegular ? amount * PricingRules.RegularRecipientRate : 0m;
    }

    /// <summary>Обчислює знижку за обсягом замовлення. Не змінює стан.</summary>
    /// <param name="amount">Сума, від якої рахується знижка, грн.</param>
    /// <returns>Розмір знижки в гривнях; 0 для малого замовлення.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Сума від’ємна.</exception>
    public static decimal VolumeDiscountOf(decimal amount)
{
    ArgumentOutOfRangeException.ThrowIfNegative(amount);
    decimal discount = 0m;
    if (amount > PricingRules.LargeOrderFrom)
    {
        discount = amount * PricingRules.LargeOrderRate;
    }
    else if (amount > PricingRules.MediumOrderFrom)
    {
        discount = amount * PricingRules.MediumOrderRate;
    }

    if (discount > PricingRules.MaxVolumeDiscount)
    {
        discount = PricingRules.MaxVolumeDiscount;
    }

    return discount;
}

    /// <summary>Обчислює знижку за кодом купона. Не змінює стан.</summary>
    /// <param name="amount">Сума, від якої рахується знижка, грн.</param>
    /// <param name="couponCode">Код купона або null.</param>
    /// <returns>Розмір знижки в гривнях; 0 для невідомого купона.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Сума від’ємна.</exception>
    public static decimal CouponDiscountOf(decimal amount, string? couponCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return couponCode switch
        {
            PricingRules.SaleCoupon
                => amount * PricingRules.CouponSaleRate,
            PricingRules.FixedCoupon
                when amount > PricingRules.CouponFixedFrom
                => PricingRules.CouponFixed,
            _ => 0m,
        };
    }

    /// <summary>Обчислює знижку вихідного дня. Не змінює стан.</summary>
    /// <param name="amount">Сума, від якої рахується знижка, грн.</param>
    /// <param name="createdOn">День створення замовлення.</param>
    /// <returns>Розмір знижки в гривнях; 0 у будній день.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Сума від’ємна.</exception>
    public static decimal WeekendDiscountOf(decimal amount, DateOnly createdOn)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return createdOn.DayOfWeek == DayOfWeek.Sunday
            ? amount * PricingRules.WeekendRate
            : 0m;
    }

    /// <summary>Обчислює плату за доставку до пункту. Не змінює стан.</summary>
    /// <param name="amount">Сума замовлення після знижок, грн.</param>
    /// <param name="deliveryPrice">Тариф доставки, грн.</param>
    /// <returns>Плата в гривнях; 0, якщо доставка безкоштовна.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Тариф від’ємний.</exception>
    public static decimal DeliveryFeeOf(decimal amount, decimal deliveryPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(deliveryPrice);
        return amount < PricingRules.FreeDeliveryFrom ? deliveryPrice : 0m;
    }

    /// <summary>Визначає наступний стан замовлення. Не змінює стан.</summary>
    /// <param name="current">Поточний стан замовлення.</param>
    /// <param name="total">Підсумок замовлення, грн.</param>
    /// <returns>Наступний стан; для кінцевих станів — поточний.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Невідомий стан.</exception>
    public static PickupStatus NextStatusOf(PickupStatus current, decimal total)
    {
        return current switch
        {
            PickupStatus.New when total > 0m => PickupStatus.Arrived,
            PickupStatus.Arrived => PickupStatus.Issued,
            PickupStatus.New or PickupStatus.Issued
                or PickupStatus.Returned or PickupStatus.Cancelled => current,
            _ => throw new ArgumentOutOfRangeException(nameof(current)),
        };
    }

    /// <summary>
    /// Обчислює підсумок замовлення та наступний стан. Не змінює жодних
    /// об’єктів і нічого не друкує.
    /// </summary>
    /// <param name="request">Запит із даними замовлення.</param>
    /// <returns>Сума до сплати в гривнях, округлена до копійок, і наступний стан.</returns>
    /// <exception cref="ArgumentNullException">Запит або одержувач — null.</exception>
    /// <exception cref="ArgumentException">Некоректна пошта чи порожній список позицій.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Некоректний номер, кількість чи ціна.</exception>
    public static TotalResult Calculate(TotalRequest request)
    {
        PickupGuards.EnsureRequestValid(request);
        decimal total = SubtotalOf(request.Lines);
        total -= RecipientDiscountOf(total, request.Recipient.IsRegular);
        total -= VolumeDiscountOf(total);
        total -= CouponDiscountOf(total, request.CouponCode);
        total -= WeekendDiscountOf(total, request.CreatedOn);
        total += DeliveryFeeOf(total, request.DeliveryPrice);
        total = request.Status == PickupStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), PricingRules.CurrencyDigits);
        return new TotalResult(total, NextStatusOf(request.Status, total));
    }
}
