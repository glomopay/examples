using GlomoCheckoutExample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register HttpClient for Glomo API
builder.Services.AddHttpClient<IGlomoPayService, GlomoPayService>(client =>
{
    var baseUrl = builder.Configuration["GlomoPay:ApiBaseUrl"] ?? "https://api.glomopay.com";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Content Security Policy configured to allow Glomo Checkout SDK
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

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
