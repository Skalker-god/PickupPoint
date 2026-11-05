using PickupPoint.Core.Domain;

namespace PickupPoint.Core.Legacy;

public static class LegacyProcessor
{
    public static decimal Process(
        int orderId, string recipientName, string recipientEmail,
        bool isRegular, List<PickupOrderLine> lines,
        DateOnly createdOn, string couponCode, string status,
        bool printToConsole, decimal deliveryPrice,
        out string newStatus)
    {
        newStatus = status;
        decimal total = 0;
        if (orderId <= 0)
        {
            Console.WriteLine("Помилка: номер замовлення");
            return -1;
        }
        if (recipientName == null || recipientName == "")
        {
            Console.WriteLine("Помилка: немає одержувача");
            return -1;
        }
        if (recipientEmail == null || !recipientEmail.Contains("@"))
        {
            Console.WriteLine("Помилка: пошта одержувача");
            return -1;
        }
        if (lines == null || lines.Count == 0)
        {
            Console.WriteLine("Помилка: немає позицій");
            return -1;
        }
        if (status != "New" && status != "Arrived" &&
            status != "Issued" && status != "Cancelled")
        {
            Console.WriteLine("Помилка: невідомий стан");
            return -1;
        }
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Quantity <= 0)
            {
                Console.WriteLine("Помилка: кількість");
                return -1;
            }
            if (lines[i].UnitPrice < 0)
            {
                Console.WriteLine("Помилка: ціна");
                return -1;
            }
            decimal sum = lines[i].Quantity * lines[i].UnitPrice;
            if (lines[i].Quantity >= 10)
                sum = sum * 0.95m;
            total = total + sum;
        }
        if (isRegular)
            total = total * 0.93m;
        if (total > 5000)
            total = total * 0.9m;
        else if (total > 2000)
            total = total * 0.95m;
        if (couponCode == "SALE10")
            total = total - total * 0.1m;
        else if (couponCode == "MINUS200" && total > 1000)
            total = total - 200;
        if (createdOn.DayOfWeek == DayOfWeek.Sunday)
            total = total * 0.98m;
        if (total < 1000)
            total = total + deliveryPrice;
        if (total < 0)
            total = 0;
        total = Math.Round(total, 2);
        if (status == "New" && total > 0)
            newStatus = "Arrived";
        else if (status == "Arrived")
            newStatus = "Issued";
        else if (status == "Cancelled")
        {
            newStatus = "Cancelled";
            total = 0;
        }
        if (printToConsole)
        {
            Console.WriteLine("Замовлення № " + orderId);
            Console.WriteLine("Одержувач: " + recipientName);
            Console.WriteLine("Пошта: " + recipientEmail);
            for (int i = 0; i < lines.Count; i++)
            {
                Console.WriteLine(lines[i].Sku + " x " +
                    lines[i].Quantity);
            }
            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }
        return total;
    }
}
