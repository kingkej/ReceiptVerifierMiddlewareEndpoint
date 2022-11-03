using AppleReceiptVerifierNET;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReceiptVerifierEndpoint;
using ReceiptVerifierMiddlewareEndpoint.Middlewares;

namespace ReceiptVerifierMiddlewareEndpoint;

public static class HostingExtensions
{
    public static IServiceCollection AddReceiptVerifierEndpointMiddleware(this IServiceCollection srv,
        Action<ReceiptVerifierMiddlewareOptions> configureEndpoint, Action<AppleReceiptVerifierOptions>? configureVerifier = null)
    {
        srv.AddOptions<ReceiptVerifierMiddlewareOptions>()
            .Configure(configureEndpoint);

        if (configureVerifier != null)
            srv.AddAppleReceiptVerifier(configureVerifier);

        return srv;
    }

    public static IApplicationBuilder UseReceiptVerifierEndpointMiddleware(this IApplicationBuilder app)
    {
        var verifier = app.ApplicationServices.GetService<IAppleReceiptVerifier>();
        if (verifier == null)
            throw new InvalidOperationException("AppleReceiptVerifier not registered.");
        
        return app.UseMiddleware<ReceiptVerifierEndpointMiddleware>();
    }
}