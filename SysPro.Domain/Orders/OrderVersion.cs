namespace SysPro.Domain.Entities;

public class OrderVersion
{
    public Guid OrderVersionId { get; set; }
    public Guid OrderId { get; set; }
    public int VersionNumber { get; set; }
    public DateTime VersionDate { get; set; }
}