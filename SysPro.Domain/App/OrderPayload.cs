using System.Text.Json.Serialization;
using CsvHelper.Configuration.Attributes;

public class OrderPayload
{
    [Name("order_external_id")]
    [JsonPropertyName("order_external_id")]
    public string OrderExternalID { get; set; }

    [Name("customer_code")]
    [JsonPropertyName("customer_code")]
    public string CustomerCode { get; set; }

    [Name("order_date")]
    [JsonPropertyName("order_date")]
    public DateTime? OrderDate { get; set; }

    [Name("line_no")]
    [JsonPropertyName("line_no")]
    public int? LineNo { get; set; }

    [Name("sku")]
    [JsonPropertyName("sku")]
    public string Sku { get; set; }

    [Name("qty")]
    [JsonPropertyName("qty")]
    public int? Quantity { get; set; }

    [Name("unit_price_cents")]
    [JsonPropertyName("unit_price_cents")]
    public int UnitPriceCents { get; set; }

    [Name("currency")]
    [JsonPropertyName("currency")]
    public string Currency { get; set; }
}