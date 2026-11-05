using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Pricing;

/// <summary>Результат розрахунку підсумку замовлення.</summary>
/// <param name="Total">Сума до сплати, грн.</param>
/// <param name="Status">Наступний стан замовлення.</param>
public sealed record TotalResult(
    decimal Total,
    PickupStatus Status);
