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
        if (string.IsNullOrWhiteSpace(sku))
        {
            return Result<PickupOrderLine>.Fail("Артикул порожній");
        }

        if (!int.TryParse(
                quantity,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int parsedQuantity)
            || parsedQuantity <= 0)
        {
            return Result<PickupOrderLine>.Fail(
                "Кількість — ціле число більше 0");
        }

        if (!decimal.TryParse(
                unitPrice,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal parsedPrice)
            || parsedPrice < 0m)
        {
            return Result<PickupOrderLine>.Fail(
                "Ціна не може бути від’ємною");
        }

        return Result<PickupOrderLine>.Ok(
            new PickupOrderLine(sku.Trim(), parsedQuantity, parsedPrice));
    }
}
