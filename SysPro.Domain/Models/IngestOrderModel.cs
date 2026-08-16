using SysPro.Domain.Entities;

namespace SysPro.Domain.Models;

public class IngestOrderModel
{
    public Orders Order { get; set; }
    public List<OrderLine>  OrderLines { get; set; }
    public OrderVersion OrderVersion { get; set; }
}