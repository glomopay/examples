namespace GlomoCheckoutExample.Models;

public class PaymentResultViewModel
{
    public string? PaymentId { get; set; }
    public string? OrderId { get; set; }
    public string? Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess { get; set; }
}
