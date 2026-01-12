using GlomoCheckoutExample.Models;

namespace GlomoCheckoutExample.Services;

public interface IGlomoPayService
{
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);
    bool VerifySignature(PaymentResponse response);
}
