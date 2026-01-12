using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using GlomoCheckoutExample.Models;

namespace GlomoCheckoutExample.Services;

public class GlomoPayService : IGlomoPayService
{
    private readonly HttpClient _httpClient;
    private readonly string _secretKey;
    private readonly ILogger<GlomoPayService> _logger;

    public GlomoPayService(HttpClient httpClient, IConfiguration configuration, ILogger<GlomoPayService> logger)
    {
        _httpClient = httpClient;
        _secretKey = configuration["GlomoPay:SecretKey"] ?? throw new InvalidOperationException("GlomoPay:SecretKey is not configured");
        _logger = logger;
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);

            var response = await _httpClient.PostAsJsonAsync("/api/v1/orders", request);
            response.EnsureSuccessStatusCode();

            var orderResponse = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
            return orderResponse ?? throw new InvalidOperationException("Failed to deserialize order response");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to create order with Glomo API");
            throw;
        }
    }

    public bool VerifySignature(PaymentResponse response)
    {
        if (string.IsNullOrEmpty(response.Signature))
        {
            _logger.LogWarning("Payment response has no signature");
            return false;
        }

        // Signature format: HMAC-SHA256(order_id|payment_id|status, secret_key)
        var payload = $"{response.OrderId}|{response.PaymentId}|{response.Status}";
        var expectedSignature = ComputeHmacSha256(payload, _secretKey);

        var isValid = string.Equals(response.Signature, expectedSignature, StringComparison.OrdinalIgnoreCase);

        if (!isValid)
        {
            _logger.LogWarning("Signature verification failed for order {OrderId}", response.OrderId);
        }

        return isValid;
    }

    private static string ComputeHmacSha256(string message, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
