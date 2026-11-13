using System.Globalization;
using PickupPoint.Core.Domain;
using PickupPoint.Core.Errors;

namespace PickupPoint.App;

/// <summary>
/// Барикада застосунку: перетворює сирі рядки консолі на типізовану позицію
/// замовлення або повертає пояснення очікуваної помилки користувача.
/// </summary>
internal static class PickupOrderLineParser
{
    private const int MaxSkuLength = 20;
    private const int MaxQuantity = 1000;
    private const decimal MaxUnitPrice = 1_000_000m;
    private const NumberStyles PriceStyles =
        NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowTrailingWhite
        | NumberStyles.AllowLeadingSign
        | NumberStyles.AllowDecimalPoint;

    /// <summary>Розбирає артикул, кількість і ціну, введені користувачем.</summary>
    /// <param name="sku">Артикул товару.</param>
    /// <param name="quantity">Кількість у вигляді рядка.</param>
    /// <param name="unitPrice">Ціна за одиницю у вигляді рядка.</param>
    /// <returns>
    /// Успішний результат із позицією або невдалий результат із поясненням.
    /// </returns>
    public static Result<PickupOrderLine> Parse(
        string sku,
        string quantity,
        string unitPrice)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > MaxSkuLength)
        {
            return Result<PickupOrderLine>.Fail(FormattableString.Invariant(
                $"Артикул — непорожній рядок до {MaxSkuLength} символів"));
        }

        if (!int.TryParse(
                quantity,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int parsedQuantity)
            || parsedQuantity is < 1 or > MaxQuantity)
        {
            return Result<PickupOrderLine>.Fail(FormattableString.Invariant(
                $"Кількість — ціле число від 1 до {MaxQuantity}"));
        }

        if (!decimal.TryParse(
                unitPrice,
                PriceStyles,
                CultureInfo.InvariantCulture,
                out decimal parsedPrice))
        {
            return Result<PickupOrderLine>.Fail(
                "Ціна — число з крапкою, наприклад 250.50");
        }

        if (parsedPrice is < 0m or > MaxUnitPrice)
        {
            return Result<PickupOrderLine>.Fail(FormattableString.Invariant(
                $"Ціна — від 0 до {MaxUnitPrice:0}"));
        }

        return Result<PickupOrderLine>.Ok(
            new PickupOrderLine(sku.Trim(), parsedQuantity, parsedPrice));
    }
}
