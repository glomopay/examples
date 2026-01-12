namespace GlomoCheckoutExample.Models;

public class CheckoutViewModel
{
    public string OrderId { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string? CallbackUrl { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}
