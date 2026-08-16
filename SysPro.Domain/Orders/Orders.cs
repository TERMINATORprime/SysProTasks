namespace SysPro.Domain.Entities;

public class Orders
{
    public Guid OrderId { get; set; }
    public string? OrderExternalID { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}