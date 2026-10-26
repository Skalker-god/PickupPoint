namespace PickupPoint.Core.Orders;

/// <summary>
/// Замовлення пункту видачі: одержувач, рядки з товарами, розрахунок суми й стан.
/// </summary>
public class Order
{
    /// <summary>Ставка податку на додану вартість.</summary>
    private const decimal VatRate = 0.2m;

    /// <summary>Поріг суми для знижки постійному одержувачу.</summary>
    private const decimal RegularDiscountThreshold = 1000m;

    /// <summary>Частка знижки постійному одержувачу.</summary>
    private const decimal RegularDiscountRate = 0.10m;

    /// <summary>Поріг суми для знижки на велике замовлення.</summary>
    private const decimal LargeOrderThreshold = 5000m;

    /// <summary>Частка знижки на велике замовлення.</summary>
    private const decimal LargeOrderDiscountRate = 0.15m;

    /// <summary>Кількість рядків, з якої діє гуртова знижка.</summary>
    private const int BulkLineCount = 10;

    /// <summary>Сума гуртової знижки, грн.</summary>
    private const decimal BulkDiscountAmount = 100m;

    /// <summary>Найбільша допустима кількість рядків у замовленні (не включно).</summary>
    private const int MaxLineCount = 100;

    /// <summary>Найменша допустима довжина імені одержувача.</summary>
    private const int MinRecipientNameLength = 3;

    /// <summary>Кількість знаків після коми в грошовій сумі.</summary>
    private const int CurrencyDecimals = 2;

    /// <summary>Код стану «нове».</summary>
    private const int StatusNew = 0;

    /// <summary>Код стану «прибуло до пункту».</summary>
    private const int StatusArrived = 1;

    /// <summary>Код стану «видано одержувачу».</summary>
    private const int StatusIssued = 2;

    /// <summary>Код стану «скасовано».</summary>
    private const int StatusCancelled = 3;

    private readonly List<string[]> _lines = new List<string[]>();

    /// <summary>
    /// Initializes a new instance of the <see cref="Order"/> class.
    /// Створює замовлення з указаним ідентифікатором і одержувачем; дата створення — поточна.
    /// </summary>
    /// <param name="id">Ідентифікатор замовлення.</param>
    /// <param name="recipientName">Ім’я одержувача.</param>
    public Order(string id, string recipientName)
    {
        Id = id;
        RecipientName = recipientName;
        CreatedAt = DateTime.Now;
    }

    /// <summary>Gets унікальний ідентифікатор замовлення.</summary>
    public string Id { get; private set; }

    /// <summary>Gets ім’я одержувача замовлення.</summary>
    public string RecipientName { get; private set; }

    /// <summary>
    /// Gets код стану: 0 — нове, 1 — прибуло до пункту, 2 — видано одержувачу, 3 — скасовано.
    /// </summary>
    public int Status { get; private set; }

    /// <summary>Gets дата й час створення замовлення.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Шукає замовлення за ідентифікатором у переданому списку.</summary>
    /// <param name="orders">Список замовлень, у якому виконується пошук.</param>
    /// <param name="orderId">Ідентифікатор шуканого замовлення.</param>
    /// <returns>Знайдене замовлення або null, якщо збігів немає.</returns>
    public static Order? FindById(List<Order> orders, string orderId)
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].Id == orderId)
            {
                return orders[i];
            }
        }

        return null;
    }

    /// <summary>Додає до замовлення рядок із товаром.</summary>
    /// <param name="sku">Артикул товару.</param>
    /// <param name="quantity">Кількість одиниць товару.</param>
    /// <param name="unitPrice">Ціна за одиницю, грн.</param>
    public void AddLine(string sku, int quantity, decimal unitPrice)
    {
        string[] line = new string[3];
        line[0] = sku;
        line[1] = quantity.ToString();
        line[2] = unitPrice.ToString();
        _lines.Add(line);
    }

    /// <summary>
    /// Обчислює суму до сплати з урахуванням знижок і податку на додану вартість.
    /// </summary>
    /// <param name="isRegularRecipient">Чи є одержувач постійним (має право на знижку).</param>
    /// <returns>Сума до сплати, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool isRegularRecipient)
    {
        decimal total = 0;
        int lineCount = 0;

        for (int i = 0; i < _lines.Count; i++)
        {
            int quantity = int.Parse(_lines[i][1]);
            decimal unitPrice = decimal.Parse(_lines[i][2]);
            total += quantity * unitPrice;
            lineCount++;
        }

        // знижка постійному одержувачу має пріоритет над знижкою на велике замовлення
        if (isRegularRecipient && total > RegularDiscountThreshold)
        {
            total *= 1m - RegularDiscountRate;
        }
        else if (total > LargeOrderThreshold)
        {
            total *= 1m - LargeOrderDiscountRate;
        }

        if (lineCount > BulkLineCount)
        {
            total -= BulkDiscountAmount;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total += total * VatRate;
        return Math.Round(total, CurrencyDecimals);
    }

    /// <summary>
    /// Змінює стан замовлення, якщо перехід дозволений правилами предметної області.
    /// </summary>
    /// <param name="newStatus">Цільовий код стану замовлення.</param>
    /// <returns>
    /// true, якщо перехід виконано; false, якщо він заборонений.
    /// </returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == StatusNew && newStatus == StatusArrived)
        {
            Status = StatusArrived;
            return true;
        }

        if (Status == StatusArrived && newStatus == StatusIssued)
        {
            Status = StatusIssued;
            return true;
        }

        if (Status == StatusNew && newStatus == StatusCancelled)
        {
            Status = StatusCancelled;
            return true;
        }

        return false;
    }

    /// <summary>Перевіряє, чи замовлення заповнене коректно.</summary>
    /// <returns>true, якщо замовлення відповідає всім обмеженням; інакше false.</returns>
    public bool IsValid()
    {
        if (!string.IsNullOrEmpty(Id)
            && RecipientName != null
            && RecipientName.Length >= MinRecipientNameLength
            && _lines.Count > 0
            && _lines.Count < MaxLineCount
            && Status >= StatusNew
            && Status <= StatusCancelled)
        {
            return true;
        }

        return false;
    }

    /// <summary>Формує текстовий звіт за рядками замовлення та підсумковою сумою.</summary>
    /// <returns>Багаторядковий текст звіту.</returns>
    public string BuildReport()
    {
        string report = string.Empty;
        for (int i = 0; i < _lines.Count; i++)
        {
            report = report + "Товар: " + _lines[i][0]
                + "; кількість: " + _lines[i][1]
                + "; ціна: " + _lines[i][2]
                + "; сума: "
                + (int.Parse(_lines[i][1]) * decimal.Parse(_lines[i][2])) + "\n";
        }

        report = report + "Разом: " + CalculateTotal(false) + "\n";
        return report;
    }
}
