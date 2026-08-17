using SysPro.Application.Ingestion;
using SysPro.Application.Repositories;
using SysPro.Domain.Models;
using SysPro.Tests.Fakes;

namespace SysPro.Tests.Ingestion;

// EXAMPLE FILE - structure and syntax reference only. Rename / rewrite as you see fit.
//
// xUnit conventions used here:
//   - one public class per unit under test, no [TestClass] attribute needed
//   - [Fact]   = a test with no parameters
//   - [Theory] + [InlineData] = the same test body run once per data row
//   - the constructor is the "setup" (runs per test), Dispose is the "teardown"
//   - async tests just return Task; no .Result / .Wait() anywhere
//   - naming: Method_Scenario_ExpectedResult
public class CsvIngestionExampleTests : IDisposable
{
    readonly string _tempDir;

    public CsvIngestionExampleTests()
    {
        // xUnit builds a NEW instance of this class for every [Fact]/[Theory] case,
        // so anything set up here is isolated per test.
        _tempDir = Path.Combine(Path.GetTempPath(), "syspro-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public async Task GetCsvContentFromFile_WithOneUnparseableRow_SkipsItAndCountsItInvalid()
    {
        // Arrange
        var path = WriteCsv(
            "order_external_id,customer_code,order_date,line_no,sku,qty,unit_price_cents,currency",
            "EXT-1001,CUST-01,2024-07-10,1,SKU-AAA,2,1299,ZAR",
            "EXT-1001,CUST-01,2024-07-10,2,SKU-BBB,not-a-number,1299,ZAR", // qty won't convert
            "EXT-1002,CUST-02,2024-07-10,1,SKU-CCC,5,500,ZAR");

        // GetCsvContentFromFile only reads the file; the repos are never touched here.
        var sut = new CSVIngestion(new FakeOrdersRepository(), new FakeAppRepository());

        // Act
        var (orders, audit) = await sut.GetCsvContentFromFile(path);

        // Assert
        Assert.Equal(3, audit.Considered);
        Assert.Equal(1, audit.Invalid);
        Assert.Equal(2, orders.Count);
        Assert.DoesNotContain(orders, o => o.Sku == "SKU-BBB");
    }

    [Theory]
    [InlineData("SKU-AAA", 2, 1299, "ZAR", "CUST-01", true)]  // identical -> unchanged
    [InlineData("SKU-ZZZ", 2, 1299, "ZAR", "CUST-01", false)] // sku changed
    [InlineData("SKU-AAA", 9, 1299, "ZAR", "CUST-01", false)] // qty changed
    [InlineData("SKU-AAA", 2, 1500, "ZAR", "CUST-01", false)] // price changed
    [InlineData("SKU-AAA", 2, 1299, "ZAR", "CUST-99", false)] // customer changed
    public void IsSameLine_ComparesEveryBusinessField(
        string sku, int qty, int priceCents, string currency, string customerCode, bool expected)
    {
        // Arrange
        var previous = new OrdersViewModel
        {
            OrderExternalId = "EXT-1001",
            LineNo          = 1,
            Sku             = "SKU-AAA",
            Quantity        = 2,
            UnitPriceCents  = 1299,
            Currency        = "ZAR",
            CustomerCode    = "CUST-01",
        };

        var incoming = new OrderPayload
        {
            OrderExternalID = "EXT-1001",
            LineNo          = 1,
            Sku             = sku,
            Quantity        = qty,
            UnitPriceCents  = priceCents,
            Currency        = currency,
            CustomerCode    = customerCode,
        };

        var sut = new OrdersRepository(context: null!); // see note (1)

        // Act
        var actual = sut.IsSameLine(previous, incoming, qty);

        // Assert
        Assert.Equal(expected, actual);
    }

    string WriteCsv(params string[] lines)
    {
        var path = Path.Combine(_tempDir, $"orders_{Guid.NewGuid():N}.csv");
        File.WriteAllLines(path, lines);
        return path;
    }
}

// ---------------------------------------------------------------------------
// NOTES / THINGS THIS EXAMPLE DELIBERATELY LEAVES TO YOU
//
// (1) `null!` for the AppDbContext / DbContext works only because neither
//     constructor touches the context, and neither method under test hits the
//     database. It is a smell, and an interviewer will likely poke at it. The
//     clean fix is constructor injection:
//
//         public CSVIngestion(IOrdersRepository orders, IAppRepository app)
//
//     ...instead of newing the concrete repositories inside the constructor.
//     That is a design decision for you to make and defend, so I have not made it.
//
// (2) The assignment explicitly asks for a test showing the same
//     order_external_id processed twice, WITH and WITHOUT changes. That test
//     belongs on ProcessCsvContext, which calls
//     _repository.GetOrdersByOrderExternalId(...) -> needs a database, unless
//     you do (1) and hand it a fake. The shape once you can inject a fake:
//
//         var existing = new List<OrdersViewModel> { /* what "day 1" left behind */ };
//         var repo     = new FakeOrdersRepository(existing);
//         var sut      = new CSVIngestion(repo, new FakeAppRepository());
//
//         var result = await sut.ProcessCsvContext(day2Payloads, audit);
//
//         // unchanged re-import  -> Assert.Empty(result)  (no version bump)
//         // changed re-import    -> Assert.Equal(2, Assert.Single(result).OrderVersion.VersionNumber)
//
//     A fake is just a class implementing IOrdersRepository whose
//     GetOrdersByOrderExternalId returns the seeded list and whose
//     InsertOrUpdateOrders echoes its input - no mocking library required.
//
// (3) Integration test (the one that must hit real SQL Server in Docker):
//     add `Microsoft.AspNetCore.Mvc.Testing` + a ProjectReference to SysPro.API,
//     add `public partial class Program { }` at the bottom of Program.cs so the
//     factory can see it, then:
//
//         public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
//         {
//             readonly HttpClient _client;
//             public OrdersApiTests(WebApplicationFactory<Program> factory) =>
//                 _client = factory.CreateClient();
//
//             [Fact]
//             public async Task PostOrder_ThenGetById_ReturnsTheOrder() { ... }
//         }
//
//     IClassFixture<T> = one shared instance across all tests in the class
//     (spin the app up once, not per test).
// ---------------------------------------------------------------------------
