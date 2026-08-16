namespace SysPro.Domain.Entities;

public class ImportAudit
{
    public Guid ImportId { get; set; }
    public string FileName { get; set; }
    public DateTime ProcessedUtc { get; set; }
    public int Considered  { get; set; }
    public int Applied { get; set; }
    public int Invalid { get; set; }
}