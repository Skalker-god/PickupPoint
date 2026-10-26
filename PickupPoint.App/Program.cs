using System.Text;
using PickupPoint.Core.Orders;

Console.OutputEncoding = Encoding.UTF8;

var order = new Order("A-1001", "Іваненко");
order.AddLine("SKU-1", 3, 250m);
order.AddLine("SKU-2", 12, 90m);
Console.WriteLine(order.CalculateTotal(false));
Console.WriteLine(order.CalculateTotal(true));
Console.WriteLine(order.IsValid());
Console.WriteLine(order.TryChangeStatus(1));
Console.Write(order.BuildReport());
