using Microsoft.AspNetCore.Mvc;
using SysPro.API.Interfaces;
using SysPro.API.Services;
using SysPro.Domain.Models;

namespace SysPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    readonly IOrdersService _ordersService;
    
    public OrdersController(IOrdersService ordersService)
        => _ordersService = ordersService;
    
    [HttpGet]
    public async Task<ActionResult<List<OrdersViewModel>>> GetOrders()
    => Ok(await _ordersService.GetAllOrdersAsync());

    [HttpGet("{id:guid}")]   
    public async Task<ActionResult<List<OrdersViewModel>>> GetOrderById(Guid id) 
        => Ok(await _ordersService.GetOrderByIdAsync(id));

    [HttpGet("byExternal")]
    public async Task<ActionResult<OrdersViewModel>> GetOrdersByExternalId([FromBody] string[] externalId) 
        => Ok(await _ordersService.GetOrdersByExternalIdAsync(externalId));

    [HttpGet("by-date-range/{startDate:datetime}&{endDate:datetime}")] 
    public async Task<ActionResult<List<OrdersViewModel>>> GetOrderByOrderDateRange([FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    => Ok(await _ordersService.GetOrderByOrderDateRangeAsync(startDate, endDate));

    [HttpPost]
    public async Task<ActionResult<List<Tuple<string, string>>>> InsertOrUpdateOrders([FromBody] List<OrderPayload> orders)
        => Ok(await _ordersService.InsertOrUpdateOrders(orders));

    [HttpGet("summary/{startDate:datetime}&{endDate:datetime}")]
    public async Task<ActionResult<List<SummaryViewModel>>> GetSummary([FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    => Ok(await _ordersService.GetOrderSummariesAsync(startDate, endDate));
}