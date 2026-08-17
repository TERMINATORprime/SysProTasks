using SysPro.Application.Ingestion;
using SysPro.Domain.Models;
using SysPro.Tests.Fakes;

namespace SysPro.Tests.Ingestion;

public class ProcessCsvContextTests
{
    static CSVIngestion BuildSut(params OrdersViewModel[] existing)
        => new(new FakeOrdersRepository(existing), new FakeAppRepository());

    static ImportAuditViewModel NewAudit() => new() { FileName = "test.csv" };
    
    static OrderPayload Line(
        string ext, int lineNo, string sku = "SKU-1", int qty = 2, int price = 100,
        string currency = "ZAR", string cust = "CUST-1", DateTime? date = null) => new()
    {
        OrderExternalID = ext,
        LineNo          = lineNo,
        Sku             = sku,
        Quantity        = qty,
        UnitPriceCents  = price,
        Currency        = currency,
        CustomerCode    = cust,
        OrderDate       = date ?? new DateTime(2024, 7, 10),
    };
    
    static OrdersViewModel Existing(
        string ext, int lineNo, int version, string sku = "SKU-1", int qty = 2,
        int price = 100, string currency = "ZAR", string cust = "CUST-1") => new()
    {
        OrderId         = Guid.NewGuid(),
        OrderExternalId = ext,
        LineNo          = lineNo,
        VersionNumber   = version,
        Sku             = sku,
        Quantity        = qty,
        UnitPriceCents  = price,
        Currency        = currency,
        CustomerCode    = cust,
        OrderLineId     = Guid.NewGuid(),
    };

    [Theory]
    [InlineData(0,   100, "SKU-1", "ZAR", "CUST-1")]  // qty <= 0
    [InlineData(2,   0,   "SKU-1", "ZAR", "CUST-1")]  // price <= 0
    [InlineData(2,   100, "",      "ZAR", "CUST-1")]  // missing sku
    [InlineData(2,   100, "SKU-1", "USD", "CUST-1")]  // wrong currency
    [InlineData(2,   100, "SKU-1", "ZAR", "")]        // missing customer
    public async Task ProcessCsvContext_SingleInvalidLine_IsRemovedAndCountedInvalid(
        int qty, int price, string sku, string currency, string cust)
    {
        var sut = BuildSut();
        var audit = NewAudit();
        var payloads = new List<OrderPayload>
        {
            Line("EXT-9001", 1, sku: sku, qty: qty, price: price, currency: currency, cust: cust),
        };

        var result = await sut.ProcessCsvContext(payloads, audit);

        Assert.Empty(result);
        Assert.Equal(1, audit.Invalid);
        Assert.Equal(0, audit.Applied);
    }

    [Fact]
    public async Task ProcessCsvContext_NullQuantity_IsRemovedAndCountedInvalid()
    {
        var sut = BuildSut();
        var audit = NewAudit();
        var bad = Line("EXT-9001", 1);
        bad.Quantity = null;

        var result = await sut.ProcessCsvContext(new List<OrderPayload> { bad }, audit);

        Assert.Empty(result);
        Assert.Equal(1, audit.Invalid);
        Assert.Equal(0, audit.Applied);
    }

    [Fact]
    public async Task ProcessCsvContext_MissingExternalId_CountsEveryLineInThatOrderInvalid()
    {
        var sut = BuildSut();
        var audit = NewAudit();
        var payloads = new List<OrderPayload> { Line("", 1), Line("", 2) };

        var result = await sut.ProcessCsvContext(payloads, audit);

        Assert.Empty(result);
        Assert.Equal(2, audit.Invalid);
        Assert.Equal(0, audit.Applied);
    }

    [Fact]
    public async Task ProcessCsvContext_MixedGoodAndBadLines_KeepsGoodAndCountsBad()
    {
        var sut = BuildSut();
        var audit = NewAudit();
        var payloads = new List<OrderPayload>
        {
            Line("EXT-9002", 1),
            Line("EXT-9002", 2, currency: "USD"),
            Line("EXT-9002", 3, qty: 0),
        };

        var result = await sut.ProcessCsvContext(payloads, audit);

        var order = Assert.Single(result);
        Assert.Single(order.OrderLines);  
        Assert.Equal(1, audit.Applied);
        Assert.Equal(2, audit.Invalid);
        Assert.Equal(1, order.OrderVersion.VersionNumber);
    }
    
    [Fact]
    public async Task ProcessCsvContext_ReimportWithChange_BumpsVersionAndReusesLineId()
    {
        var stored = Existing("EXT-1001", lineNo: 1, version: 1, qty: 2);
        var sut = BuildSut(stored);
        var audit = NewAudit();
        
        var payloads = new List<OrderPayload> { Line("EXT-1001", 1, qty: 9) };

        var result = await sut.ProcessCsvContext(payloads, audit);

        var order = Assert.Single(result);
        Assert.Equal(2, order.OrderVersion.VersionNumber);
        var line = Assert.Single(order.OrderLines);
        Assert.Equal(9, line.Quantity);
        Assert.Equal(stored.OrderLineId, line.OrderLineId);
        Assert.Equal(1, audit.Applied);
        Assert.Equal(0, audit.Invalid);
    }

    [Fact]
    public async Task ProcessCsvContext_ReimportWithOneOfTwoLinesChanged_BumpsVersionOnceAppliesOnlyChanged()
    {
        var l1 = Existing("EXT-1001", lineNo: 1, version: 3, qty: 2);
        var l2 = Existing("EXT-1001", lineNo: 2, version: 3, qty: 5);
        var sut = BuildSut(l1, l2);
        var audit = NewAudit();

        var payloads = new List<OrderPayload>
        {
            Line("EXT-1001", 1, qty: 2),
            Line("EXT-1001", 2, qty: 8),
        };

        var result = await sut.ProcessCsvContext(payloads, audit);

        var order = Assert.Single(result);
        Assert.Equal(4, order.OrderVersion.VersionNumber);
        var line = Assert.Single(order.OrderLines);
        Assert.Equal(2, line.LineNo);
        Assert.Equal(8, line.Quantity);
        Assert.Equal(1, audit.Applied);
        Assert.Equal(0, audit.Invalid);
    }
    
    [Fact]
    public async Task ProcessCsvContext_ReimportIdentical_ProducesNothingAndDoesNotBumpVersion()
    {
        var stored = Existing("EXT-1001", lineNo: 1, version: 1);
        var sut = BuildSut(stored);
        var audit = NewAudit();
        
        var payloads = new List<OrderPayload> { Line("EXT-1001", 1) };

        var result = await sut.ProcessCsvContext(payloads, audit);

        Assert.Empty(result);
        Assert.Equal(0, audit.Applied);
        Assert.Equal(0, audit.Invalid);
    }
}