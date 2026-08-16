using SysPro.Domain.Models;

namespace SysPro.API.Interfaces;

public interface IOrdersService
{
    Task<List<OrdersViewModel>> GetAllOrdersAsync();
    Task<List<OrdersViewModel>> GetOrderByIdAsync(Guid id);
    Task<List<OrdersViewModel>> GetOrdersByExternalIdAsync(string[] id);
    Task<List<OrdersViewModel>> GetOrderByOrderDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<SummaryViewModel>> GetOrderSummariesAsync(DateTime startDate, DateTime endDate);
    
    Task<List<Tuple<string, string>>> InsertOrUpdateOrders(List<OrderPayload> order);
}