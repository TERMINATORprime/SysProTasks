using CsvHelper.Configuration.Attributes;

public class OrderCSV
{
    [Name("order_external_id")] public string OrderExternalID { get; set; }

    [Name("customer_code")] public string CustomerCode { get; set; }

    [Name("order_date")] public DateTime? OrderDate { get; set; }

    [Name("line_no")] public int? LineNo { get; set; }

    [Name("sku")] public string Sku { get; set; }

    [Name("qty")] public int? Quantity { get; set; }

    [Name("unit_price_cents")] public decimal? UnitPriceCents { get; set; }

    [Name("currency")] public string Currency { get; set; }
}