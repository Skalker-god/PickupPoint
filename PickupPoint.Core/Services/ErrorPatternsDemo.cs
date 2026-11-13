using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;
using PickupPoint.Core.Errors;

namespace PickupPoint.Core.Services;

/// <summary>
/// Навчальні фрагменти обробки помилок, перенесені з методички на типи
/// PickupPoint. Після виправлення методи не проковтують винятків, зберігають
/// стек викликів і звільняють файлові ресурси.
/// </summary>
public sealed class ErrorPatternsDemo
{
    private readonly IPickupOrderRepository _repository;

    /// <summary>Створює демонстраційний клас над сховищем замовлень.</summary>
    /// <param name="repository">Сховище замовлень.</param>
    /// <exception cref="ArgumentNullException">Сховище — null.</exception>
    public ErrorPatternsDemo(IPickupOrderRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    /// <summary>
    /// Підраховує додатні кількості без винятків як керування потоком.
    /// </summary>
    /// <param name="rows">Рядки з кількістю.</param>
    /// <returns>Кількість додатних чисел.</returns>
    public static int CountValidQuantities(string[] rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        int valid = 0;
        foreach (string row in rows)
        {
            if (int.TryParse(row, out int quantity) && quantity > 0)
            {
                valid++;
            }
        }

        return valid;
    }

    /// <summary>
    /// Експортує замовлення у файл; <c>using</c> закриває потік навіть при
    /// винятку всередині блоку.
    /// </summary>
    /// <param name="path">Шлях до файлу звіту.</param>
    /// <param name="order">Замовлення, яке експортують.</param>
    /// <exception cref="ArgumentNullException">Замовлення — null.</exception>
    public static void ExportReport(string path, PickupOrder order)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(order);
        using StreamWriter writer = new(path);
        writer.WriteLine($"Замовлення {order.Id}");
        writer.WriteLine(order.Lines.Count);
    }

    /// <summary>
    /// Знаходить замовлення або порушує правило його існування.
    /// Порожній <c>catch</c> і повернення <c>null</c> прибрано.
    /// </summary>
    /// <param name="orderId">Ідентифікатор замовлення.</param>
    /// <returns>Знайдене замовлення.</returns>
    /// <exception cref="DomainRuleException">Замовлення не знайдено.</exception>
    public PickupOrder Load(int orderId)
    {
        return _repository.GetById(orderId)
            ?? throw new DomainRuleException(
                "order.exists",
                $"Замовлення {orderId} не знайдено");
    }

    /// <summary>
    /// Скасовує замовлення, перехоплюючи лише конкретний тип і зберігаючи
    /// первинний виняток через <c>innerException</c>.
    /// </summary>
    /// <param name="orderId">Ідентифікатор замовлення.</param>
    /// <param name="reason">Непорожня причина скасування.</param>
    /// <exception cref="ArgumentException">Причина порожня.</exception>
    /// <exception cref="DomainRuleException">
    /// Замовлення не знайдено або його стан забороняє скасування.
    /// </exception>
    public void Cancel(int orderId, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        PickupOrder order = Load(orderId);
        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainRuleException(
                "order.cancellable",
                $"Стан {order.Status}: замовлення {orderId} не скасовується.",
                ex);
        }
    }
}
