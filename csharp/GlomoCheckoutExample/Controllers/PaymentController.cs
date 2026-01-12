using Microsoft.AspNetCore.Mvc;
using GlomoCheckoutExample.Models;
using GlomoCheckoutExample.Services;

namespace GlomoCheckoutExample.Controllers;

public class PaymentController : Controller
{
    private readonly IGlomoPayService _glomoPayService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IGlomoPayService glomoPayService,
        IConfiguration configuration,
        ILogger<PaymentController> logger)
    {
        _glomoPayService = glomoPayService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        try
        {
            // Create order via Glomo API
            // Note: In production, customer_id should come from your system's customer records
            var orderResponse = await _glomoPayService.CreateOrderAsync(new CreateOrderRequest
            {
                CustomerId = _configuration["GlomoPay:CustomerId"] ?? "cust_demo_example",
                Amount = 1000, // $10.00 in cents
                Currency = "USD",
                InvoiceDescription = "Demo Product - Glomo Checkout Example",
                PaymentMethods = ["card"]
            });

            _logger.LogInformation("Created order {OrderId}", orderResponse.Id);

            // Prepare view model with checkout configuration
            var viewModel = new CheckoutViewModel
            {
                OrderId = orderResponse.Id,
                PublicKey = _configuration["GlomoPay:PublicKey"] ?? throw new InvalidOperationException("GlomoPay:PublicKey is not configured"),
                ProductName = "Demo Product",
                Amount = 10.00m,
                Currency = "USD"
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create checkout");
            return RedirectToAction("Failure", new { error = "Failed to create order. Please try again." });
        }
    }

    [HttpPost]
    public IActionResult HandlePaymentResponse([FromBody] PaymentResponse response)
    {
        _logger.LogInformation("Received payment response for order {OrderId}, status: {Status}",
            response.OrderId, response.Status);

        // Verify signature for successful payments
        if (response.Status == "success")
        {
            if (_glomoPayService.VerifySignature(response))
            {
                return Json(new
                {
                    redirectUrl = Url.Action("Success", new
                    {
                        paymentId = response.PaymentId,
                        orderId = response.OrderId
                    })
                });
            }

            _logger.LogWarning("Signature verification failed for order {OrderId}", response.OrderId);
            return Json(new
            {
                redirectUrl = Url.Action("Failure", new { error = "Payment verification failed" })
            });
        }

        // Handle failure
        return Json(new
        {
            redirectUrl = Url.Action("Failure", new
            {
                errorCode = response.ErrorCode,
                error = response.ErrorMessage ?? "Payment failed"
            })
        });
    }

    [HttpGet]
    public IActionResult Success(string paymentId, string orderId)
    {
        var viewModel = new PaymentResultViewModel
        {
            PaymentId = paymentId,
            OrderId = orderId,
            Status = "success",
            IsSuccess = true
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Failure(string? errorCode, string? error)
    {
        var viewModel = new PaymentResultViewModel
        {
            ErrorCode = errorCode,
            ErrorMessage = error ?? "An unknown error occurred",
            IsSuccess = false
        };

        return View(viewModel);
    }
}
