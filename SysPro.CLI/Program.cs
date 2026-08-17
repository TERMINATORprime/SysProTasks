using Microsoft.Extensions.DependencyInjection;
using SysPro.Application.Ingestion;
using SysPro.DB;

var services = new ServiceCollection();

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                       ?? throw new InvalidOperationException("ConnectionStrings__Default is not set.");

services.AddInfrastructure(connectionString);
services.AddScoped<CSVIngestion>();

using var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();
var ingestion = scope.ServiceProvider.GetRequiredService<CSVIngestion>();

//set to folder path
var folder = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "csvData");

var path = buildPath(folder ,folder);
var csvFiles = CsvFileDiscovery.GetCsvFiles(path);

static string buildPath(params string[] segments)
{
    var path = Path.Combine(segments);
    return Path.GetFullPath(path);
}

try
{
    foreach (var csvFile in csvFiles)
    {
        Console.WriteLine($"Importing {csvFile}:");
        var csvResult = await ingestion.GetCsvContentFromFile(csvFile);

        var audit = csvResult.Item2;

        var result = await ingestion.ProcessCsvContext(csvResult.Item1, audit);

        var insertResult = await ingestion.InsertData(result);

        audit.ProcessedUtc = DateTime.UtcNow;
        
        var insertedImportAudit = await ingestion.InsertImportAudit(audit);

        Console.WriteLine($"file {csvFile} has been inserted; Applied: {audit.Applied}, Invalid: {audit.Invalid}");
    }   
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

Console.ReadLine();