namespace SysPro.Domain.Models;

public class CreateOrderViewModel
{
    public string? OrderExternalId { get; set; }
    public string? CustomerCode { get; set; } = "";
    public DateTime OrderDate { get; set; }
    public int LineNo { get; set; }
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceCents { get; set; }
    public string? Currency { get; set; }
}