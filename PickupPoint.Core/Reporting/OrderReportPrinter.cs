namespace PickupPoint.Core.Reporting;

/// <summary>Виводить готовий звіт у заданий потік.</summary>
public sealed class OrderReportPrinter
{
    private readonly TextWriter _output;

    /// <summary>Створює принтер для заданого потоку виводу.</summary>
    /// <param name="output">Потік, куди пишуть звіт.</param>
    /// <exception cref="ArgumentNullException">Потік — null.</exception>
    public OrderReportPrinter(TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _output = output;
    }

    /// <summary>Записує звіт у потік виводу.</summary>
    /// <param name="report">Текст звіту.</param>
    /// <exception cref="ArgumentException">Звіт порожній.</exception>
    public void Print(string report)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(report);
        _output.Write(report);
    }
}
