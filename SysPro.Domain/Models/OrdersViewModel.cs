namespace SysPro.Domain.Models;

public class OrdersViewModel
{
    public Guid OrderId { get; set; }
    
    public string? OrderExternalId { get; set; }
    public DateTime OrderDate { get; set; }
    
    public DateTime OrderCreatedDate { get; set; }
    public DateTime? OrderModifiedDate { get; set; }
    
    public Guid OrderLineId { get; set; }

    public string? CustomerCode { get; set; } = "";
    public int LineNo { get; set; }
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public int UnitPriceCents { get; set; }
    public string? Currency { get; set; }
    
    public DateTime OrderLineCreateDate { get; set; }
    public DateTime OrderLineUpdateDate { get; set; }
    
    
    public Guid OrderVersionId { get; set; }
    public int VersionNumber { get; set; }
    public DateTime VersionDate { get; set; }
}