using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SysPro.Application.Interfaces;
using SysPro.DB.Persistence;
using SysPro.Domain.Models;

namespace SysPro.Application.Repositories;

public class OrdersRepository : IOrdersRepository
{
    readonly AppDbContext _dbContext;
    public OrdersRepository(AppDbContext context)
    {
        _dbContext = context;
    }
    
    public async Task<List<OrdersViewModel>> GetOrdersByOrderExternalId(string externalOrderIds)
    {
        return await _dbContext.Set<OrdersViewModel>()
            .FromSqlInterpolated($"EXEC Orders.GetOrdersByExternalId {externalOrderIds}").ToListAsync();
    }

    public async Task<List<IngestOrderModel>> InsertOrUpdateOrders(List<IngestOrderModel> orders)
    {
        if (orders.Count == 0) return orders;

        var pOrders = new SqlParameter("@Orders", SqlDbType.Structured)
            { TypeName = "Orders.OrderTvp",        Value = BuildOrdersTable(orders) };
        var pLines = new SqlParameter("@OrderLines", SqlDbType.Structured)
            { TypeName = "Orders.OrderLineTvp",    Value = BuildLinesTable(orders) };
        var pVersions = new SqlParameter("@OrderVersions", SqlDbType.Structured)
            { TypeName = "Orders.OrderVersionTvp", Value = BuildVersionsTable(orders) };
        
        var connection = (SqlConnection)_dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        
        if (shouldClose) await connection.OpenAsync();

        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Orders.InsertOrUpdateOrders";
            cmd.Parameters.AddRange([pOrders, pLines, pVersions]);
            
            var idByExternal = new Dictionary<string, Guid>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                idByExternal[reader.GetString(1)] = reader.GetGuid(0);
            
            foreach (var o in orders)
                if (o.Order.OrderExternalID is { } ext && idByExternal.TryGetValue(ext, out var id))
                    o.Order.OrderId = id;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
        return orders;
    }

    public async Task<List<OrdersViewModel>> GetAllOrdersAsync()
        => await _dbContext.Set<OrdersViewModel>()
            .FromSqlInterpolated($"EXEC Orders.GetAllOrders")
            .ToListAsync();

    public async Task<List<OrdersViewModel>> GetOrderByIdAsync(Guid id)
        => await _dbContext.Set<OrdersViewModel>()
            .FromSqlInterpolated($"EXEC Orders.GetOrderById @OrderId={id}")
            .ToListAsync();

    public async Task<List<OrdersViewModel>> GetOrderByOrderDateRangeAsync(DateTime startDate, DateTime endDate)
        => await _dbContext.Set<OrdersViewModel>()
            .FromSqlInterpolated($"EXEC Orders.GetOrderByOrderDateRange @StartDate={startDate}, @EndDate={endDate}")
            .ToListAsync();
    
    public async Task<List<SummaryViewModel>> GetOrderSummariesAsync(DateTime startDate, DateTime endDate)
     => await  _dbContext.Set<SummaryViewModel>()
         .FromSqlInterpolated($"EXEC Orders.GetOrderSummary @StartDate={startDate}, @EndDate={endDate}")
         .ToListAsync();

    private static DataTable BuildOrdersTable(List<IngestOrderModel> orders)
    {
        var t = new DataTable();
        t.Columns.Add("OrderExternalID", typeof(string));
        t.Columns.Add("OrderDate", typeof(DateTime));
        foreach (var o in orders)
            t.Rows.Add(o.Order.OrderExternalID, o.Order.OrderDate);
        return t;
    }
    
    private static DataTable BuildLinesTable(List<IngestOrderModel> orders)
    {
        var t = new DataTable();
        t.Columns.Add("OrderLineId", typeof(Guid));
        t.Columns.Add("OrderExternalID", typeof(string));
        t.Columns.Add("LineNo", typeof(int));
        t.Columns.Add("CustomerCode", typeof(string));
        t.Columns.Add("Sku", typeof(string));
        t.Columns.Add("Quantity", typeof(int));
        t.Columns.Add("UnitPriceCents", typeof(decimal));
        t.Columns.Add("Currency", typeof(string));
        foreach (var o in orders)
        foreach (var l in o.OrderLines)
            t.Rows.Add(l.OrderLineId, o.Order.OrderExternalID, l.LineNo,
                (object?)l.CustomerCode ?? DBNull.Value, l.Sku, l.Quantity,
                l.UnitPriceCents, (object?)l.Currency ?? DBNull.Value);
        return t;
    }
    
    private static DataTable BuildVersionsTable(List<IngestOrderModel> orders)
    {
        var t = new DataTable();
        t.Columns.Add("OrderExternalID", typeof(string));
        t.Columns.Add("VersionNumber", typeof(int));
        t.Columns.Add("VersionDate", typeof(DateTime));
        foreach (var o in orders)
            t.Rows.Add(o.Order.OrderExternalID, o.OrderVersion.VersionNumber, o.OrderVersion.VersionDate);
        return t;
    }
    
    public bool IsSameLine(OrdersViewModel previous, OrderPayload line, int quantity)
        => previous.Sku            == line.Sku
           && previous.Quantity       == quantity
           && previous.UnitPriceCents == line.UnitPriceCents 
           && previous.Currency       == line.Currency
           && previous.CustomerCode   == line.CustomerCode;
}