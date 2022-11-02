using ReceiptVerifierEndpoint.Middlewares;
using AppleReceiptVerifierNET;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ReceiptVerifierEndpoint;

public static class HostingExtensions
{
    public static IServiceCollection AddApnsVerifierEndpoint(this IServiceCollection srv,
        Action<ReceiptVerifierMiddlewareOptions> configureEndpoint, Action<AppleReceiptVerifierOptions>? configureVerifier = null)
    {
        srv.AddOptions<ReceiptVerifierMiddlewareOptions>()
            .Configure(configureEndpoint);

        if (configureVerifier != null)
            srv.AddAppleReceiptVerifier(configureVerifier);

        return srv;
    }

    public static IApplicationBuilder UseApnsVerifierEndpoint(this IApplicationBuilder app)
    {
        var verifier = app.ApplicationServices.GetService<IAppleReceiptVerifier>();
        if (verifier == null)
            throw new InvalidOperationException("а кто регать верифаер будет");
        
        return app.UseMiddleware<ReceiptVerifierEndpointMiddleware>();
    }
}