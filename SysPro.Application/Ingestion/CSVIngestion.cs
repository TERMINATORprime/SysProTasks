using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using SysPro.Application.Interfaces;
using SysPro.Domain.Entities;
using SysPro.Domain.Models;

namespace SysPro.Application.Ingestion;

public class CSVIngestion
{
    private readonly IOrdersRepository _repository;
    private readonly IAppRepository _appRepository;

    public CSVIngestion(IOrdersRepository repository, IAppRepository appRepository)
    {
        _repository = repository;
        _appRepository = appRepository;
    }
    
    CsvConfiguration _config = null;
    private CsvConfiguration GetCsvConfiguration()
    {
        if (_config == null)
        {
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                HeaderValidated = null,
                IgnoreBlankLines =  false,
            };
        }
        
        return _config;
    }

    public async Task<Tuple<List<OrderPayload>, ImportAuditViewModel>> GetCsvContentFromFile(string filePath)
    {
        GetCsvConfiguration();
        
        var audit = new ImportAuditViewModel()
        {
            FileName = Path.GetFileName(filePath),
            Applied = 0,
            Invalid = 0,
            Considered = 0
        };
        
        var csvOrders = new List<OrderPayload>();
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, _config);
        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            audit.Considered++;
            try
            {
                csvOrders.Add(csv.GetRecord<OrderPayload>());
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
            }
        }
        
        return new Tuple<List<OrderPayload>, ImportAuditViewModel>(csvOrders, audit);
    }

    public async Task<List<IngestOrderModel>> ProcessCsvContext(List<OrderPayload> csvOrders, ImportAuditViewModel audit)
    {
        var orders = csvOrders.GroupBy(x => x.OrderExternalID);

        var orderIds = orders.Select(x => x.Key)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToList();
        var existingOrders = await _repository.GetOrdersByOrderExternalId(string.Join(",", orderIds));

        var result = new List<IngestOrderModel>();
        
        foreach (var order in orders)
        {
            var externalId = order.Key;
            var orderDate  = order.FirstOrDefault(x => x.OrderDate != null)?.OrderDate;
            
            if (string.IsNullOrWhiteSpace(externalId) || orderDate is null)
            {
                audit.Invalid += order.Count();
                continue;
            }
            
            var previousOrder = existingOrders.FirstOrDefault(x => x.OrderExternalId == externalId);

            var ingestOrderModel = new IngestOrderModel
            {
                Order = new Orders
                {
                    OrderExternalID = externalId,
                    OrderDate       = orderDate.Value,
                    CreatedDate     = DateTime.UtcNow,
                    ModifiedDate    = DateTime.UtcNow,
                },
                OrderLines   = [],
                OrderVersion = new OrderVersion { VersionDate = DateTime.UtcNow },
            };

            var usedLineNo = new HashSet<int>();
            var seq = 1;
            
            foreach (var line in order.OrderBy(x => x.LineNo))
            {
                var lineNo = line.LineNo ?? seq;
                seq++;

                if (!usedLineNo.Add(lineNo))
                {
                    audit.Invalid++;
                    continue;
                }

                if (line.Quantity is null or <= 0 || line.UnitPriceCents <= 0 
                                                  || string.IsNullOrWhiteSpace(line.Sku) || line.Currency != "ZAR")
                {
                    audit.Invalid++;
                    continue;
                }

                if (string.IsNullOrEmpty(line.CustomerCode))
                {
                    audit.Invalid++;
                    continue;
                }
                
                var quantity     = line.Quantity.Value;
                var previousLine = existingOrders.FirstOrDefault(x =>
                    x.OrderExternalId == externalId && x.LineNo == lineNo);

                if (previousLine != null && _repository.IsSameLine(previousLine, line, quantity))
                    continue;
                
                var orderLine = new OrderLine
                {
                    CustomerCode   = line.CustomerCode,
                    LineNo         = lineNo,
                    Sku            = line.Sku,
                    Quantity       = quantity,
                    UnitPriceCents = line.UnitPriceCents,
                    Currency       = line.Currency,
                    UpdateDate     = DateTime.UtcNow,
                };
                
                if (previousLine != null)
                    orderLine.OrderLineId = previousLine.OrderLineId;
                
                ingestOrderModel.OrderLines.Add(orderLine);
                audit.Applied++;
            }
            
            if (ingestOrderModel.OrderLines.Count == 0)
                continue;
            
            ingestOrderModel.OrderVersion.VersionNumber =
                previousOrder is null ? 1 : previousOrder.VersionNumber + 1;
            
            result.Add(ingestOrderModel);
        }
        return result;
    }

    public async Task<List<IngestOrderModel>> InsertData(List<IngestOrderModel> data)
    {
        return await _repository.InsertOrUpdateOrders(data);
    }

    public async Task<bool> InsertImportAudit(ImportAuditViewModel auditViewModel)
    {
        return await _appRepository.InsertAudit(auditViewModel);
    }
    
}