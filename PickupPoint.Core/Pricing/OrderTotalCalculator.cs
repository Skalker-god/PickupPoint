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
        if (amount > PricingRules.LargeOrderFrom)
        {
            return amount * PricingRules.LargeOrderRate;
        }

        if (amount > PricingRules.MediumOrderFrom)
        {
            return amount * PricingRules.MediumOrderRate;
        }

        return 0m;
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

    /// <summary>Тимчасовий диспетчер: викликає кроки в потрібному порядку.</summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="recipientName">Ім’я одержувача.</param>
    /// <param name="recipientEmail">Пошта одержувача.</param>
    /// <param name="isRegular">Ознака постійного одержувача.</param>
    /// <param name="lines">Позиції замовлення.</param>
    /// <param name="createdOn">День створення замовлення.</param>
    /// <param name="couponCode">Код купона або null.</param>
    /// <param name="status">Поточний стан.</param>
    /// <param name="printToConsole">Чи друкувати звіт.</param>
    /// <param name="deliveryPrice">Тариф доставки, грн.</param>
    /// <param name="newStatus">Наступний стан.</param>
    /// <returns>Підсумок замовлення, грн.</returns>
    public static decimal Process(
        int orderId,
        string recipientName,
        string recipientEmail,
        bool isRegular,
        IReadOnlyList<PickupOrderLine> lines,
        DateOnly createdOn,
        string? couponCode,
        PickupStatus status,
        bool printToConsole,
        decimal deliveryPrice,
        out PickupStatus newStatus)
    {
        PickupGuards.EnsureHeaderValid(orderId, recipientName, recipientEmail);
        decimal total = SubtotalOf(lines);
        total -= RecipientDiscountOf(total, isRegular);
        total -= VolumeDiscountOf(total);
        total -= CouponDiscountOf(total, couponCode);
        total -= WeekendDiscountOf(total, createdOn);
        total += DeliveryFeeOf(total, deliveryPrice);
        total = status == PickupStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), PricingRules.CurrencyDigits);
        newStatus = NextStatusOf(status, total);

        if (printToConsole)
        {
            Console.WriteLine("Замовлення № " + orderId);
            Console.WriteLine("Одержувач: " + recipientName);
            foreach (PickupOrderLine line in lines)
            {
                Console.WriteLine(line.Sku + " x " +
                    line.Quantity);
            }

            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }

        return total;
    }
}
