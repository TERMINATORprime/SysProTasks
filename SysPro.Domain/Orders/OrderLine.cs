namespace SysPro.Domain.Entities;

public class OrderLine
{
    public Guid OrderLineId { get; set; }

    public string? CustomerCode { get; set; } = "";
    public int LineNo { get; set; }
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceCents { get; set; }
    public string? Currency { get; set; }
    
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    
    public Guid OrderID { get; set; }
    
}