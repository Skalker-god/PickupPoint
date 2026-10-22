namespace PickupPoint.Core.Domain;

/// <summary>Одержувач замовлень у пункті видачі.</summary>
public sealed class Recipient
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;
}
