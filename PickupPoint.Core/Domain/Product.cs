namespace PickupPoint.Core.Domain;

/// <summary>
/// Товар із довідника пункту видачі; залишок змінюється лише методами.
/// </summary>
public sealed class Product
{
    private int _stock;

    /// <summary>
    /// Створює товар і перевіряє його інваріант.
    /// </summary>
    /// <param name="sku">Артикул товару.</param>
    /// <param name="title">Назва товару.</param>
    /// <param name="price">Ціна за одиницю, більша за нуль.</param>
    /// <param name="stock">Початковий залишок, невід’ємний.</param>
    /// <exception cref="ArgumentException">Артикул або назва порожні.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Ціна не додатна або залишок від’ємний.
    /// </exception>
    public Product(string sku, string title, decimal price, int stock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
        ArgumentOutOfRangeException.ThrowIfNegative(stock);
        Sku = sku;
        Title = title;
        Price = price;
        _stock = stock;
    }

    /// <summary>Артикул товару.</summary>
    public string Sku { get; }

    /// <summary>Назва товару.</summary>
    public string Title { get; }

    /// <summary>Ціна за одиницю, грн.</summary>
    public decimal Price { get; }

    /// <summary>Поточний залишок товару в пункті видачі.</summary>
    public int Stock => _stock;

    /// <summary>Приймає товар на склад.</summary>
    /// <param name="quantity">Кількість одиниць, більша за нуль.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Кількість не додатна.
    /// </exception>
    public void Receive(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        _stock += quantity;
    }

    /// <summary>Видає товар зі складу, не порушуючи інваріант залишку.</summary>
    /// <param name="quantity">Кількість одиниць, більша за нуль.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Кількість не додатна.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// На складі недостатньо товару.
    /// </exception>
    public void Issue(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        if (quantity > _stock)
        {
            throw new InvalidOperationException(
                "На складі недостатньо товару.");
        }

        _stock -= quantity;
    }
}
