namespace SysPro.Application.Ingestion;

public static class CsvFileDiscovery
{
    public static IReadOnlyList<string> GetCsvFiles(string folder)
    {
        if (!Directory.Exists(folder))
            return Array.Empty<string>();

        return Directory.EnumerateFiles(folder)
            .Where(f => Path.GetExtension(f).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();
    }
}
