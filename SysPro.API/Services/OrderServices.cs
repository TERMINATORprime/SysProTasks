using SysPro.API.Interfaces;
using SysPro.Application.Interfaces;
using SysPro.Domain.Entities;
using SysPro.Domain.Models;

namespace SysPro.API.Services;

public class OrderServices(IOrdersRepository ordersRepository) : IOrdersService
{
    public async Task<List<OrdersViewModel>> GetAllOrdersAsync()
        => await ordersRepository.GetAllOrdersAsync();

    public async Task<List<OrdersViewModel>> GetOrdersByExternalIdAsync(string[] id)
        => await ordersRepository.GetOrdersByOrderExternalId(string.Join(",", id));

    public async Task<List<OrdersViewModel>> GetOrderByIdAsync(Guid id)
        => await ordersRepository.GetOrderByIdAsync(id);

    public async Task<List<OrdersViewModel>> GetOrderByOrderDateRangeAsync(DateTime startDate, DateTime endDate)
        => await ordersRepository.GetOrderByOrderDateRangeAsync(startDate, endDate);
    
    public async Task<List<SummaryViewModel>> GetOrderSummariesAsync(DateTime startDate, DateTime endDate)
        => await ordersRepository.GetOrderSummariesAsync(startDate, endDate);

    public async Task<List<Tuple<string, string>>> InsertOrUpdateOrders(List<OrderPayload> ordersPayload)
    {
        var orders = ordersPayload.GroupBy(x => x.OrderExternalID);

        var orderIds = orders.Select(x => x.Key)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .ToList();
        var existingOrders = await ordersRepository.GetOrdersByOrderExternalId(string.Join(",", orderIds));

        var ordersToInsert = new List<IngestOrderModel>();
        
        foreach (var order in orders)
        {
            var externalId = order.Key;
            var orderDate  = order.FirstOrDefault(x => x.OrderDate != null)?.OrderDate;
            
            if (string.IsNullOrWhiteSpace(externalId) || orderDate is null)
            {
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
                    continue;
                }

                if (line.Quantity is null or <= 0 || line.UnitPriceCents <= 0 
                                                  || string.IsNullOrWhiteSpace(line.Sku) || line.Currency != "ZAR")
                {
                    continue;
                }
                
                if (string.IsNullOrEmpty(line.CustomerCode))
                {
                    continue;
                }
                
                var quantity     = line.Quantity.Value;
                var previousLine = existingOrders.FirstOrDefault(x =>
                    x.OrderExternalId == externalId && x.LineNo == lineNo);

                if (previousLine != null && ordersRepository.IsSameLine(previousLine, line, quantity))
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
            }
            
            if (ingestOrderModel.OrderLines.Count == 0)
                continue;
            
            ingestOrderModel.OrderVersion.VersionNumber =
                previousOrder is null ? 1 : previousOrder.VersionNumber + 1;
            
            ordersToInsert.Add(ingestOrderModel);
        }

        ordersToInsert = await ordersRepository.InsertOrUpdateOrders(ordersToInsert);

        var result = new List<Tuple<string, string>>();
        
        foreach (var model in ordersToInsert)
        {
            result.Add(new (model.Order.OrderExternalID, model.Order.OrderId.ToString()));
        }
        
        return result;
    }
}