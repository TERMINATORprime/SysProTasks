using Microsoft.EntityFrameworkCore;
using SysPro.Application.Ingestion;
using SysPro.DB.Persistence;
using SysPro.DB.Repositories;
using SysPro.Domain.Models;
using Testcontainers.MsSql;

namespace SysPro.Tests.Integration;

[Trait("Category", "Integration")]
public class OrderIngestionIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sql = new MsSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _sql.StartAsync();
        await using var ctx = NewContext();
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _sql.DisposeAsync().AsTask();

    private AppDbContext NewContext()
        => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_sql.GetConnectionString())
            .Options);

    [Fact]
    public async Task ImportA_ThenReimportB_CommitsExpectedAuditsAndDbState()
    {
        var ext = $"EXT-{Guid.NewGuid():N}";

        var auditA = await RunImport("import_a.csv", ext);

        Assert.Equal(3, auditA.Considered);
        Assert.Equal(3, auditA.Applied);
        Assert.Equal(0, auditA.Invalid);

        var afterA = await ReadOrder(ext);
        Assert.Equal(new[] { 1 }, afterA.Versions);
        Assert.Equal(3, afterA.Lines.Count);
        Assert.Equal(10, Qty(afterA, lineNo: 1));
        Assert.Equal(5,  Qty(afterA, lineNo: 2));
        Assert.Equal(2,  Qty(afterA, lineNo: 3));

        var auditB = await RunImport("import_b.csv", ext);

        Assert.Equal(3, auditB.Considered);
        Assert.Equal(1, auditB.Applied);
        Assert.Equal(0, auditB.Invalid);

        var afterB = await ReadOrder(ext);
        Assert.Equal(new[] { 1, 2 }, afterB.Versions);
        Assert.Equal(3, afterB.Lines.Count);
        
        Assert.Equal(25, Qty(afterB, lineNo: 1));
        Assert.Equal(5,  Qty(afterB, lineNo: 2));
        Assert.Equal(2,  Qty(afterB, lineNo: 3));
    }
    
    private async Task<ImportAuditViewModel> RunImport(string csvName, string ext)
    {
        var path = MaterializeCsv(csvName, ext);

        await using var ctx = NewContext();
        var ingestion = new CSVIngestion(new OrdersRepository(ctx), new AppRepository(ctx));

        var (payloads, audit) = await ingestion.GetCsvContentFromFile(path);
        var toInsert = await ingestion.ProcessCsvContext(payloads, audit);
        await ingestion.InsertData(toInsert);
        audit.ProcessedUtc = DateTime.UtcNow;
        await ingestion.InsertImportAudit(audit);

        return audit;
    }
    
    private async Task<OrderState> ReadOrder(string ext)
    {
        await using var ctx = NewContext();
        var order = await ctx.Orders.SingleAsync(o => o.OrderExternalID == ext);

        var lines = await ctx.OrderLines
            .Where(l => l.OrderID == order.OrderId)
            .OrderBy(l => l.LineNo)
            .Select(l => new LineState(l.LineNo, l.Quantity))
            .ToListAsync();

        var versions = await ctx.OrderVersions
            .Where(v => v.OrderId == order.OrderId)
            .OrderBy(v => v.VersionNumber)
            .Select(v => v.VersionNumber)
            .ToListAsync();

        return new OrderState(lines, versions);
    }

    private static int Qty(OrderState state, int lineNo)
        => state.Lines.Single(l => l.LineNo == lineNo).Qty;
    
    private static string MaterializeCsv(string csvName, string ext)
    {
        var src = Path.Combine(AppContext.BaseDirectory, "TestData", csvName);
        var text = File.ReadAllText(src).Replace("__EXT__", ext);
        var dst = Path.Combine(Path.GetTempPath(), $"syspro-int-{Guid.NewGuid():N}-{csvName}");
        File.WriteAllText(dst, text);
        return dst;
    }

    private sealed record OrderState(IReadOnlyList<LineState> Lines, IReadOnlyList<int> Versions);
    private sealed record LineState(int LineNo, int Qty);
}
