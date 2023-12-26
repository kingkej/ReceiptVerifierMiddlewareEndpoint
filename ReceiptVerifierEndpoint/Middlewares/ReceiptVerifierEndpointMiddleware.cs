using System.Text.Json;
using AppleReceiptVerifierNET;
using Microsoft.Extensions.Options;
using ReceiptVerifierEndpoint.Models;

namespace ReceiptVerifierMiddlewareEndpoint.Middlewares;

public class ReceiptVerifierEndpointMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ReceiptVerifierMiddlewareOptions _options;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public ReceiptVerifierEndpointMiddleware(RequestDelegate next, IOptions<ReceiptVerifierMiddlewareOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Equals(_options.Path, StringComparison.InvariantCultureIgnoreCase))
        {
            var body = await JsonSerializer.DeserializeAsync<ReceiptDto>(context.Request.Body,
                SerializerOptions);
            if (body == null)
            {
                context.Response.StatusCode = 400;
            }
            else
            {
                IAppleReceiptVerifier verifier = _options.VerifierName == null
                    ? context.RequestServices.GetRequiredService<IAppleReceiptVerifier>()
                    : context.RequestServices.GetRequiredService<IAppleReceiptVerifierResolver>()
                        .Resolve(_options.VerifierName);
                var receiptInfo = await verifier.VerifyReceiptAsync(body.Receipt);
                await context.Response.WriteAsync(receiptInfo.RawJson);
            }
        }
        else
        {
            await _next(context);
        }
    }
}
