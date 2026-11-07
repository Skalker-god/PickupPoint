using System.Diagnostics.CodeAnalysis;

namespace PickupPoint.Core.Errors;

/// <summary>
/// Результат операції, яка може завершитися очікуваною помилкою користувача.
/// Містить або успішне значення, або текст пояснення.
/// </summary>
/// <typeparam name="T">Тип успішного значення.</typeparam>
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Фабричні методи Ok і Fail — канонічний контракт типу-результату.")]
public readonly struct Result<T>
{
    private Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Ознака успішного завершення операції.</summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Успішне значення; для невдалого результату — <c>default</c>.
    /// Читають лише після перевірки <see cref="IsSuccess"/>.
    /// </summary>
    public T? Value { get; }

    /// <summary>Пояснення помилки; для успішного результату — порожній рядок.</summary>
    public string Error { get; }

    /// <summary>Створює успішний результат із значенням.</summary>
    /// <param name="value">Значення, яке повертають викликачеві.</param>
    /// <returns>Успішний результат.</returns>
    public static Result<T> Ok(T value) => new(true, value, string.Empty);

    /// <summary>Створює невдалий результат із поясненням.</summary>
    /// <param name="error">Непорожнє пояснення помилки.</param>
    /// <returns>Невдалий результат без значення.</returns>
    /// <exception cref="ArgumentException">Пояснення порожнє.</exception>
    public static Result<T> Fail(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new Result<T>(false, default, error);
    }
}
