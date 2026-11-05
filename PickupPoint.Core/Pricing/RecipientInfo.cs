namespace PickupPoint.Core.Pricing;

/// <summary>Дані одержувача, потрібні для розрахунку.</summary>
/// <param name="Name">Ім’я одержувача.</param>
/// <param name="Email">Пошта одержувача.</param>
/// <param name="IsRegular">Ознака постійного одержувача.</param>
public sealed record RecipientInfo(
    string Name,
    string Email,
    bool IsRegular);
