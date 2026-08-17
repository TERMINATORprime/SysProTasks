using SysPro.Application.Ingestion;

namespace SysPro.Tests.Ingestion;

public class CsvFileDiscoveryTests : IDisposable
{
    readonly string _dir;

    public CsvFileDiscoveryTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "syspro-discovery", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    void Touch(string name) => File.WriteAllText(Path.Combine(_dir, name), "");

    [Fact]
    public void GetCsvFiles_ReturnsOnlyCsvFiles()
    {
        Touch("orders.csv");
        Touch("more_orders.CSV");
        Touch("notes.txt");
        Touch("data.json");
        Touch("orders.csv.bak");
        Touch("noextension");

        var files = CsvFileDiscovery.GetCsvFiles(_dir);

        Assert.Equal(2, files.Count);
        Assert.All(files, f => Assert.Equal(".csv", Path.GetExtension(f), ignoreCase: true));
        Assert.Contains(files, f => Path.GetFileName(f) == "orders.csv");
        Assert.Contains(files, f => Path.GetFileName(f) == "more_orders.CSV");
    }

    [Fact]
    public void GetCsvFiles_MissingFolder_ReturnsEmpty()
        => Assert.Empty(CsvFileDiscovery.GetCsvFiles(Path.Combine(_dir, "does-not-exist")));
}