namespace PickupPoint.Core.Domain;

/// <summary>
/// Агрегат замовлення: прихована колекція позицій і контрольовані переходи
/// станів.
/// </summary>
public sealed class PickupOrder
{
    /// <summary>Строк повернення замовлення після видачі, днів.</summary>
    private const int ReturnWindowDays = 14;

    private readonly List<PickupOrderLine> _lines = new();

    /// <summary>
    /// Створює нове замовлення у стані <see cref="PickupStatus.New"/>.
    /// </summary>
    /// <param name="id">Ідентифікатор замовлення, більший за нуль.</param>
    /// <param name="recipientId">Ідентифікатор одержувача, більший за нуль.</param>
    /// <param name="createdAt">Момент створення замовлення.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Ідентифікатор замовлення або одержувача не додатний.
    /// </exception>
    public PickupOrder(int id, int recipientId, DateTimeOffset createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(recipientId);
        Id = id;
        RecipientId = recipientId;
        CreatedAt = createdAt;
        Status = PickupStatus.New;
    }

    /// <summary>Унікальний ідентифікатор замовлення.</summary>
    public int Id { get; }

    /// <summary>Ідентифікатор одержувача замовлення.</summary>
    public int RecipientId { get; }

    /// <summary>Момент створення замовлення.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Поточний стан замовлення; змінюється лише методами переходів.</summary>
    public PickupStatus Status { get; private set; }

    /// <summary>Момент видачі замовлення; null, поки замовлення не видано.</summary>
    public DateTimeOffset? IssuedAt { get; private set; }

    /// <summary>Позиції замовлення, доступні клієнтові лише для читання.</summary>
    public IReadOnlyList<PickupOrderLine> Lines => _lines;

    /// <summary>Додає позицію до нового замовлення.</summary>
    /// <param name="line">Позиція, яку додають.</param>
    /// <exception cref="ArgumentNullException">Позиція — null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Замовлення вже не у стані <see cref="PickupStatus.New"/>.
    /// </exception>
    public void AddLine(PickupOrderLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        if (Status != PickupStatus.New)
        {
            throw new InvalidOperationException(
                "Позиції додають лише до нового замовлення.");
        }

        _lines.Add(line);
    }

    /// <summary>Переводить замовлення зі стану New у стан Arrived.</summary>
    /// <exception cref="InvalidOperationException">
    /// Перехід заборонений або замовлення не має позицій.
    /// </exception>
    public void MarkArrived()
    {
        EnsureCurrentStatus(PickupStatus.New);
        EnsureHasLines();
        Status = PickupStatus.Arrived;
    }

    /// <summary>Переводить замовлення зі стану Arrived у стан Issued.</summary>
    /// <param name="issuedAt">Момент видачі замовлення.</param>
    /// <exception cref="InvalidOperationException">
    /// Перехід заборонений або замовлення не має позицій.
    /// </exception>
    public void Issue(DateTimeOffset issuedAt)
    {
        EnsureCurrentStatus(PickupStatus.Arrived);
        EnsureHasLines();
        Status = PickupStatus.Issued;
        IssuedAt = issuedAt;
    }

    /// <summary>Скасовує замовлення, яке ще не видано.</summary>
    /// <exception cref="InvalidOperationException">
    /// Замовлення вже видано, повернено або скасовано.
    /// </exception>
    public void Cancel()
    {
        if (Status is not (PickupStatus.New or PickupStatus.Arrived))
        {
            throw new InvalidOperationException(
                "Скасувати можна лише невидане замовлення.");
        }

        Status = PickupStatus.Cancelled;
    }

    /// <summary>Повертає видане замовлення в межах строку повернення.</summary>
    /// <param name="returnedOn">Дата повернення.</param>
    /// <exception cref="InvalidOperationException">
    /// Замовлення не видано або строк повернення минув.
    /// </exception>
    public void Return(DateOnly returnedOn)
    {
        EnsureCurrentStatus(PickupStatus.Issued);
        DateOnly issuedOn = DateOnly.FromDateTime(IssuedAt!.Value.Date);
        if (returnedOn.DayNumber - issuedOn.DayNumber > ReturnWindowDays)
        {
            throw new InvalidOperationException(
                "Строк повернення замовлення минув.");
        }

        Status = PickupStatus.Returned;
    }

    private void EnsureCurrentStatus(PickupStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"Перехід із стану {Status} заборонено.");
        }
    }

    private void EnsureHasLines()
    {
        if (_lines.Count == 0)
        {
            throw new InvalidOperationException(
                "Замовлення не містить жодної позиції.");
        }
    }
}
