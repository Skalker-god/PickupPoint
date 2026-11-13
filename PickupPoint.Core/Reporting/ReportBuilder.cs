using System.Text;

namespace PickupPoint.Core.Reporting;

/// <summary>Формує текст простого звіту з готових рядків.</summary>
public sealed class ReportBuilder
{
    /// <summary>Заголовок звіту.</summary>
    public string Title { get; init; } = "Звіт";

    /// <summary>Збирає звіт: заголовок великими літерами та рядки.</summary>
    /// <param name="rows">Рядки звіту.</param>
    /// <returns>Готовий текст звіту.</returns>
    /// <exception cref="ArgumentNullException">Рядки не задано.</exception>
    public string Build(IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        StringBuilder text = new();
        text.AppendLine(Title.ToUpperInvariant());
        foreach (string row in rows)
        {
            text.AppendLine(row);
        }

        return text.ToString();
    }
}
