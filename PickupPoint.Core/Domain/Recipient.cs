namespace PickupPoint.Core.Domain;

/// <summary>Одержувач замовлень у пункті видачі.</summary>
public sealed class Recipient
{
    /// <summary>Gets унікальний ідентифікатор одержувача.</summary>
    public int Id { get; init; }

    /// <summary>Gets прізвище та ім’я одержувача.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets контактний телефон одержувача.</summary>
    public string Phone { get; init; } = string.Empty;
}
