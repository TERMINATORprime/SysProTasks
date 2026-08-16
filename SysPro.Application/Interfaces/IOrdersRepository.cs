using SysPro.Domain.Models;

namespace SysPro.Application.Interfaces;

public interface IOrdersRepository
{
    Task<List<OrdersViewModel>> GetOrdersByOrderExternalId(string externalOrderIds);
    Task<List<IngestOrderModel>> InsertOrUpdateOrders(List<IngestOrderModel> orders);
    
    Task<List<OrdersViewModel>> GetAllOrdersAsync();
    Task<List<OrdersViewModel>> GetOrderByIdAsync(Guid id);
    Task<List<OrdersViewModel>> GetOrderByOrderDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<SummaryViewModel>> GetOrderSummariesAsync(DateTime startDate, DateTime endDate);

    public bool IsSameLine(OrdersViewModel previous, OrderPayload line, int quantity);
}