using System;
using System.Collections.Generic;
using System.Linq;

namespace PickupPoint.Core.Orders;

/// <summary>
/// Замовлення пункту видачі: одержувач, рядки з товарами, розрахунок суми й стан.
/// </summary>
public class Order
{
    private readonly List<string[]> _lines = new List<string[]>();

    // public string prim; // примітка, поки не треба

    /// <summary>
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

    /// <summary>Унікальний ідентифікатор замовлення.</summary>
    public string Id { get; private set; }

    /// <summary>Ім’я одержувача замовлення.</summary>
    public string RecipientName { get; private set; }

    /// <summary>
    /// Код стану: 0 — нове, 1 — прибуло до пункту, 2 — видано одержувачу, 3 — скасовано.
    /// </summary>
    public int Status { get; private set; }

    /// <summary>Дата й час створення замовлення.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Шукає замовлення за ідентифікатором у переданому списку.</summary>
    /// <param name="orders">Список замовлень, у якому виконується пошук.</param>
    /// <param name="orderId">Ідентифікатор шуканого замовлення.</param>
    /// <returns>Знайдене замовлення або null, якщо збігів немає.</returns>
    public static Order FindById(List<Order> orders, string orderId)
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
    /// <param name="isRegularRecipient">Ознака постійного одержувача, якому належить знижка.</param>
    /// <returns>Сума до сплати, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool isRegularRecipient)
    {
        decimal total = 0;
        int lineCount = 0;

        for (int i = 0; i < _lines.Count; i++)
        {
            int quantity = int.Parse(_lines[i][1]);
            decimal unitPrice = decimal.Parse(_lines[i][2]);
            total = total + quantity * unitPrice;
            lineCount = lineCount + 1;
        }

        // if (sum1 > 500) { sum1 = sum1 - 50; } // стара знижка
        // знижка постійному одержувачу має пріоритет над знижкою на велике замовлення
        if (isRegularRecipient == true && total > 1000)
        {
            total = total * 0.9m;
        }
        else if (total > 5000)
        {
            total = total * 0.85m;
        }
        else
        {
            total = total;
        }

        if (lineCount > 10)
        {
            total = total - 100;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total = total + total * 0.2m;
        return Math.Round(total, 2);
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
        if (Status == 0 && newStatus == 1)
        {
            Status = 1;
            return true;
        }

        if (Status == 1 && newStatus == 2)
        {
            Status = 2;
            return true;
        }

        if (Status == 0 && newStatus == 3)
        {
            Status = 3;
            return true;
        }

        return false;
    }

    /// <summary>Перевіряє, чи замовлення заповнене коректно.</summary>
    /// <returns>true, якщо замовлення відповідає всім обмеженням; інакше false.</returns>
    public bool IsValid()
    {
        if (Id != null
            && Id != ""
            && RecipientName != null
            && RecipientName.Length > 2
            && _lines.Count > 0
            && _lines.Count < 100
            && Status >= 0
            && Status <= 3)
        {
            return true;
        }

        return false;
    }

    /// <summary>Формує текстовий звіт за рядками замовлення та підсумковою сумою.</summary>
    /// <returns>Багаторядковий текст звіту.</returns>
    public string BuildReport()
    {
        string report = "";
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
