namespace PickupPoint.Core.Domain;

/// <summary>
/// Незмінний одержувач замовлень у пункті видачі.
/// </summary>
public sealed record Recipient
{
    /// <summary>
    /// Створює одержувача та перевіряє його інваріант.
    /// </summary>
    /// <param name="id">Ідентифікатор одержувача, більший за нуль.</param>
    /// <param name="name">Прізвище та ім’я одержувача.</param>
    /// <param name="phone">Контактний телефон.</param>
    /// <param name="email">Пошта, що містить символ @.</param>
    /// <exception cref="ArgumentException">
    /// Ім’я, телефон або пошта порожні чи пошта без символу @.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Ідентифікатор не додатний.
    /// </exception>
    public Recipient(int id, string name, string phone, string email)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        if (!email.Contains('@'))
        {
            throw new ArgumentException(
                "Пошта має містити символ @.",
                nameof(email));
        }

        Id = id;
        Name = name;
        Phone = phone;
        Email = email;
    }

    /// <summary>Унікальний ідентифікатор одержувача.</summary>
    public int Id { get; }

    /// <summary>Прізвище та ім’я одержувача.</summary>
    public string Name { get; }

    /// <summary>Контактний телефон одержувача.</summary>
    public string Phone { get; }

    /// <summary>Пошта одержувача.</summary>
    public string Email { get; }

    /// <summary>Ознака постійного одержувача.</summary>
    public bool IsRegular { get; init; }
}
