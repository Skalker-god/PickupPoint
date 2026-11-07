#pragma warning disable CA1031 // навмисний антиприклад: catch (Exception)
#pragma warning disable CA2200 // навмисний антиприклад: throw ex
#pragma warning disable CA2000 // навмисний антиприклад: StreamWriter без using
using PickupPoint.Core.Abstractions;
using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Services;

/// <summary>
/// Навчальні фрагменти «до виправлення»: чотири антипатерни обробки помилок,
/// перенесені з методички на типи PickupPoint. Після виправлення методи
/// зберігають ту саму поведінку, але не приховують дефектів.
/// </summary>
public sealed class ErrorPatternsDemo
{
    private readonly IPickupOrderRepository _repository;

    /// <summary>Створює демонстраційний клас над сховищем замовлень.</summary>
    /// <param name="repository">Сховище замовлень.</param>
    public ErrorPatternsDemo(IPickupOrderRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// АНТИПАТЕРН 4: виняток як керування потоком замість <c>TryParse</c>.
    /// </summary>
    /// <param name="rows">Рядки з кількістю.</param>
    /// <returns>Кількість додатних чисел.</returns>
    public static int CountValidQuantities(string[] rows)
    {
        int valid = 0;
        foreach (string row in rows)
        {
            try
            {
                int quantity = int.Parse(row);
                if (quantity > 0)
                {
                    valid++;
                }
            }
            catch (FormatException)
            {
            }
        }

        return valid;
    }

    /// <summary>
    /// АНТИПАТЕРН ресурсів: <see cref="StreamWriter"/> не звільняється через
    /// <c>using</c>, тому при винятку файл лишається відкритим.
    /// </summary>
    /// <param name="path">Шлях до файлу звіту.</param>
    /// <param name="order">Замовлення, яке експортують.</param>
    public static void ExportReport(string path, PickupOrder order)
    {
        StreamWriter writer = new(path);
        writer.WriteLine($"Замовлення {order.Id}");
        writer.WriteLine(order.Lines.Count);
        writer.Close();
    }

    /// <summary>
    /// АНТИПАТЕРН 1: порожній <c>catch</c> і повернення <c>null</c>.
    /// </summary>
    /// <param name="orderId">Ідентифікатор замовлення.</param>
    /// <returns>Замовлення або <c>null</c>.</returns>
    public PickupOrder? Load(int orderId)
    {
        try
        {
            return _repository.GetById(orderId);
        }
        catch (Exception)
        {
        }

        return null;
    }

    /// <summary>
    /// АНТИПАТЕРНИ 2 і 3: широке перехоплення та <c>throw ex</c>, що стирає
    /// початковий стек викликів.
    /// </summary>
    /// <param name="orderId">Ідентифікатор замовлення.</param>
    /// <param name="reason">Причина скасування.</param>
    public void Cancel(int orderId, string reason)
    {
        try
        {
            PickupOrder order = _repository.GetById(orderId)!;
            order.Cancel();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Щось пішло не так");
            throw ex;
        }
    }
}
