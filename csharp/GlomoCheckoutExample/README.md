# Glomo Checkout Example - C# / ASP.NET Core MVC

This example demonstrates how to integrate the Glomo Checkout SDK into a C# ASP.NET Core MVC application.

## Prerequisites

**With Docker (Recommended):**
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

**Without Docker:**
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

**Both methods require:**
- Glomo account with API keys from [app.glomopay.com](https://app.glomopay.com/api-keys-and-webhooks/api-keys)

## Quick Start with Docker

1. **Set your API keys:**

   ```bash
   export GLOMO_PUBLIC_KEY=your_public_key_here
   export GLOMO_SECRET_KEY=your_secret_key_here
   ```

2. **Build and run:**

   ```bash
   docker compose up --build
   ```

3. **Open your browser:**

   Navigate to [http://localhost:8080](http://localhost:8080)

## Running Without Docker

1. **Set your API keys:**

   ```bash
   # On Linux/macOS
   export GlomoPay__PublicKey=your_public_key_here
   export GlomoPay__SecretKey=your_secret_key_here

   # On Windows (PowerShell)
   $env:GlomoPay__PublicKey="your_public_key_here"
   $env:GlomoPay__SecretKey="your_secret_key_here"
   ```

   Or update `appsettings.json` directly (not recommended for production).

2. **Restore and run:**

   ```bash
   dotnet restore
   dotnet run
   ```

3. **Open your browser:**

   Navigate to [https://localhost:5001](https://localhost:5001) or [http://localhost:5000](http://localhost:5000)

## Project Structure

```
GlomoCheckoutExample/
├── Controllers/
│   ├── HomeController.cs       # Home page controller
│   └── PaymentController.cs    # Payment flow controller
├── Models/
│   ├── CreateOrderRequest.cs   # Order creation request
│   ├── CreateOrderResponse.cs  # Order creation response
│   ├── CheckoutViewModel.cs    # Checkout page view model
│   ├── PaymentResponse.cs      # Payment callback response
│   └── PaymentResultViewModel.cs # Result page view model
├── Services/
│   ├── IGlomoPayService.cs     # Service interface
│   └── GlomoPayService.cs      # Glomo API integration
├── Views/
│   ├── Home/
│   │   └── Index.cshtml        # Product display page
│   ├── Payment/
│   │   ├── Checkout.cshtml     # Checkout with SDK
│   │   ├── Success.cshtml      # Payment success page
│   │   └── Failure.cshtml      # Payment failure page
│   └── Shared/
│       └── _Layout.cshtml      # Shared layout
├── wwwroot/css/
│   └── site.css                # Styles
├── Program.cs                  # App configuration
├── appsettings.json           # Configuration
├── Dockerfile                 # Docker build
└── docker-compose.yml         # Docker orchestration
```

## How It Works

### 1. Server-Side: Create Order

When the user clicks "Buy Now", the `PaymentController.Checkout()` action creates an order via the Glomo API:

```csharp
var orderResponse = await _glomoPayService.CreateOrderAsync(new CreateOrderRequest
{
    CustomerId = "cust_xxx",  // Required: Your customer's ID
    Amount = 1000,            // $10.00 in cents
    Currency = "USD",
    PaymentMethods = ["card"]
});
```

The API is called with Bearer token authentication:
```
POST /api/v1/orders
Authorization: Bearer <your_secret_key>
```

### 2. Client-Side: Initialize SDK

The checkout page loads the Glomo Checkout SDK with the order details:

```javascript
import { GlomoCheckoutApi } from 'https://glomopay-checkout-sdk.web.app/index.js';

const checkout = new GlomoCheckoutApi({
    orderId: 'order_xxx',
    publicKey: 'live_xxx'
});

checkout.open();
```

### 3. Handle Payment Response

The SDK triggers events for success/failure which are sent to the server for signature verification:

```javascript
checkout.on('payment.success', function(response) {
    fetch('/Payment/HandlePaymentResponse', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(response)
    })
    .then(res => res.json())
    .then(data => {
        if (data.redirectUrl) window.location.href = data.redirectUrl;
    });
});
```

### 4. Server-Side: Verify Signature

The server verifies the payment signature using HMAC-SHA256:

```csharp
public bool VerifySignature(PaymentResponse response)
{
    var payload = $"{response.OrderId}|{response.PaymentId}|{response.Status}";
    var expectedSignature = ComputeHmacSha256(payload, _secretKey);
    return response.Signature == expectedSignature;
}
```

## Configuration Options

| Environment Variable | Description | Default |
|---------------------|-------------|---------|
| `GlomoPay__PublicKey` | Your Glomo public key | Required |
| `GlomoPay__SecretKey` | Your Glomo secret key (JWT token) | Required |
| `GlomoPay__CustomerId` | Customer ID for orders | Required for production |
| `GlomoPay__ApiBaseUrl` | Glomo API base URL | `https://api.glomopay.com` |

**For Sandbox Testing:**
- Use sandbox API keys (prefixed with `test_`) - the API gateway routes automatically based on key type

## Security Notes

- **Never expose your secret key** on the client side
- Always **verify the payment signature** on your server before fulfilling orders
- Use **environment variables** for API keys in production
- The public key can safely be used in client-side code

## Content Security Policy (CSP)

If your application uses Content Security Policy headers, you must allow the Glomo Checkout SDK domains. Add these directives to your CSP:

```csharp
// In Program.cs
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://glomopay-checkout-sdk.web.app; " +
        "frame-src 'self' https://glomopay-checkout-sdk.web.app https://*.glomopay.com; " +
        "connect-src 'self' https://glomopay-checkout-sdk.web.app https://*.glomopay.com; " +
        "style-src 'self' 'unsafe-inline';");
    await next();
});
```

| CSP Directive | Required Domains | Purpose |
|---------------|------------------|---------|
| `script-src` | `https://glomopay-checkout-sdk.web.app` | Load checkout SDK JavaScript |
| `frame-src` | `https://glomopay-checkout-sdk.web.app https://*.glomopay.com` | Checkout modal iframe |
| `connect-src` | `https://glomopay-checkout-sdk.web.app https://*.glomopay.com` | API calls and source maps |

**Troubleshooting:** If you see "This content is blocked" error, check browser DevTools Console for CSP violation messages.

## Documentation

- [Glomo Checkout Integration Guide](https://docs.glomopay.com/product-guide/payin/checkout)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)

## Support

For issues with this example, please open an issue in this repository.

For Glomo API support, contact [developer@glomopay.com](mailto:developer@glomopay.com)
