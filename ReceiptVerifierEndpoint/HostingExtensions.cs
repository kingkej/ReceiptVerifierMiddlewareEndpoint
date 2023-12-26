using AppleReceiptVerifierNET;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
        var options = app.ApplicationServices
            .GetRequiredService<IOptions<ReceiptVerifierMiddlewareOptions>>().Value;
        using var scope = app.ApplicationServices.CreateScope();
        if (options.VerifierName == null)
        {
            _ = scope.ServiceProvider.GetRequiredService<IAppleReceiptVerifier>();
        }
        else
        {
            var resolver = scope.ServiceProvider.GetRequiredService<IAppleReceiptVerifierResolver>();
            _ = resolver.Resolve(options.VerifierName) ?? throw new InvalidOperationException(
                    $"AppleReceiptVerifier named {options.VerifierName} is not registered.");
        }

        return app.UseMiddleware<ReceiptVerifierEndpointMiddleware>();
    }
}