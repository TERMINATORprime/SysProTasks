using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using SysPro.Domain.Entities;

string folder = "csvData";
var applicationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
var path = buildPath(applicationPath ,folder);
var csvFiles = Directory.GetFiles(path, "*.csv");
var orders = new Dictionary<string,List<OrderCSV>>();
List<ImportAudit> importAudits = [];
var errors = new List<string>();

static string buildPath(params string[] segments)
{
    var path = Path.Combine(segments);
    return Path.GetFullPath(path);
}

var configCsv = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = true,
    TrimOptions = TrimOptions.Trim,
    MissingFieldFound = null,
    HeaderValidated = null,
    IgnoreBlankLines =  false,
};

foreach (var csvFile in csvFiles.Reverse())
{
    var audit = new ImportAudit()
    {
        FileName = Path.GetFileNameWithoutExtension(csvFile),
        ProcessedUtc = DateTime.UtcNow,
    };
    var csvOrders = new List<OrderCSV>();
    using var reader = new StreamReader(csvFile);
    using var csv = new CsvReader(reader, configCsv);
    csv.Read();
    csv.ReadHeader();

    while (csv.Read())
    {
        audit.Considered++;
        try
        {
            csvOrders.Add(csv.GetRecord<OrderCSV>());
            audit.Applied++;
        }
        catch (Exception e)
        {
            audit.Invalid++;
            var message = "";
            if (csv.Context.Parser != null)
            {
                message = $"File: {audit.FileName} Row {csv.Context.Parser.Row}: {e.Message}";
            }
            else
            {
                message = $"File: {audit.FileName} Row {audit.Considered}: {e.Message}";
            }
            errors.Add(message);
            //Console.WriteLine(message);
        }
    }
    
    orders.Add(audit.FileName, csvOrders);
    importAudits.Add(audit);
}

foreach (var audit in importAudits)
{
    Console.WriteLine(audit.FileName);
    Console.WriteLine(audit.Considered);
    Console.WriteLine(audit.Invalid);
}