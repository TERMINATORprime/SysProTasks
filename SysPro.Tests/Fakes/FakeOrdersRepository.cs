using SysPro.Application.Interfaces;
using SysPro.DB.Repositories;
using SysPro.Domain.Models;

namespace SysPro.Tests.Fakes;

public class FakeOrdersRepository : IOrdersRepository
{
    private static readonly OrdersRepository RealComparer = new(null!);

    private readonly List<OrdersViewModel> _existing;

    public List<IngestOrderModel>? LastInsertArg { get; private set; }

    public FakeOrdersRepository(IEnumerable<OrdersViewModel>? existing = null)
        => _existing = existing?.ToList() ?? new List<OrdersViewModel>();

    public Task<List<OrdersViewModel>> GetOrdersByOrderExternalId(string externalOrderIds)
        => Task.FromResult(_existing.ToList());

    public Task<List<IngestOrderModel>> InsertOrUpdateOrders(List<IngestOrderModel> orders)
    {
        LastInsertArg = orders;
        foreach (var o in orders.Where(o => o.Order.OrderId == Guid.Empty))
            o.Order.OrderId = Guid.NewGuid();
        return Task.FromResult(orders);
    }

    public bool IsSameLine(OrdersViewModel previous, OrderPayload line, int quantity)
        => RealComparer.IsSameLine(previous, line, quantity);

    public Task<List<OrdersViewModel>> GetAllOrdersAsync() => throw new NotImplementedException();
    public Task<List<OrdersViewModel>> GetOrderByIdAsync(Guid id) => throw new NotImplementedException();
    public Task<List<OrdersViewModel>> GetOrderByOrderDateRangeAsync(DateTime startDate, DateTime endDate) => throw new NotImplementedException();
    public Task<List<SummaryViewModel>> GetOrderSummariesAsync(DateTime startDate, DateTime endDate) => throw new NotImplementedException();
}