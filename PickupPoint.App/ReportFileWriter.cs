namespace PickupPoint.App;

/// <summary>
/// Записує готовий текст звіту у файл. Потік звільняється через
/// <c>using</c> навіть тоді, коли запис перервано винятком.
/// </summary>
internal static class ReportFileWriter
{
    /// <summary>Записує звіт у файл, перезаписуючи попередній вміст.</summary>
    /// <param name="path">Непорожній шлях до файлу.</param>
    /// <param name="report">Непорожній текст звіту.</param>
    /// <exception cref="ArgumentException">Шлях або текст порожні.</exception>
    /// <exception cref="UnauthorizedAccessException">Немає прав на запис.</exception>
    /// <exception cref="IOException">Файл недоступний або диск переповнений.</exception>
    public static void Export(string path, string report)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(report);
        using StreamWriter writer = new(path);
        writer.Write(report);
    }
}
