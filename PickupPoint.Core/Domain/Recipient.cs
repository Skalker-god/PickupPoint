namespace PickupPoint.Core.Domain;

/// <summary>Одержувач замовлень у пункті видачі.</summary>
public sealed class Recipient
{
    /// <summary>Унікальний ідентифікатор одержувача.</summary>
    public int Id { get; init; }

    /// <summary>Прізвище та ім’я одержувача.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Контактний телефон одержувача.</summary>
    public string Phone { get; init; } = string.Empty;
}
