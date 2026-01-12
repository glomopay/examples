using System.Text.Json.Serialization;

namespace GlomoCheckoutExample.Models;

public class CreateOrderRequest
{
    /// <summary>
    /// Required: Unique identifier of the customer linked to the order
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Required: Positive integer representing the smallest currency unit (e.g., cents)
    /// </summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    /// <summary>
    /// Required: ISO 4217 currency code (USD, EUR, or GBP)
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Optional: Invoice identifier for reconciliation
    /// </summary>
    [JsonPropertyName("invoice_number")]
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Optional: Invoice details/description
    /// </summary>
    [JsonPropertyName("invoice_description")]
    public string? InvoiceDescription { get; set; }

    /// <summary>
    /// Optional: Available checkout payment methods (bank_transfer, pay_via_bank, card)
    /// </summary>
    [JsonPropertyName("payment_methods")]
    public List<string>? PaymentMethods { get; set; }

    /// <summary>
    /// Optional: Additional metadata as key-value pairs
    /// </summary>
    [JsonPropertyName("notes")]
    public Dictionary<string, string>? Notes { get; set; }
}
