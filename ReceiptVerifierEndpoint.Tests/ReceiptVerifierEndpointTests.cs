using System.Text;
using AppleReceiptVerifierNET;
using AppleReceiptVerifierNET.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using ReceiptVerifierMiddlewareEndpoint;
using ReceiptVerifierMiddlewareEndpoint.Middlewares;

namespace ReceiptVerifierEndpoint.Tests;

public class ReceiptVerifierEndpointTests
{
    [Fact]
    public async Task Ensure_Invoke_Async_Writes_Response_Body()
    {
        const string requestUrl = "/api/receiptInfo";

        var options = new OptionsWrapper<ReceiptVerifierMiddlewareOptions>(new ReceiptVerifierMiddlewareOptions
        {
            Path = requestUrl
        });

        var receiptVerifierMock = new Mock<IAppleReceiptVerifier>(MockBehavior.Strict);
        receiptVerifierMock.Setup(v => v.VerifyReceiptAsync("abc", false))
            .ReturnsAsync(new VerifyReceiptResponse(null, null, null, null, null, null!, 0)
            {
                RawJson = "abc"
            });
        var receiptVerifierEndpoint =
            new ReceiptVerifierEndpointMiddleware(null!, options);

        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = requestUrl,
                Body = new MemoryStream(Encoding.ASCII.GetBytes("{\"Receipt\":\"abc\"}"))
            },
            Response =
            {
                Body = new MemoryStream()
            },
            RequestServices = new ServiceCollection()
                .AddTransient<IAppleReceiptVerifier>(_ => receiptVerifierMock.Object)
                .BuildServiceProvider()
        };

        await receiptVerifierEndpoint.InvokeAsync(context);

        var responseBody = context.Response.Body;
        responseBody.Position = 0;
        var responseStr = await new StreamReader(responseBody).ReadToEndAsync();
        Assert.Equal("abc", responseStr);
    }

    [Fact]
    public async Task Ensure_Next_Delegate_Executes_If_Wrong_Request_Url()
    {
        const string requestUrl = "/api/receiptInfo";
        const string wrongRequestUrl = "/receiptInfo";

        var options = new OptionsWrapper<ReceiptVerifierMiddlewareOptions>(new ReceiptVerifierMiddlewareOptions
        {
            Path = requestUrl
        });



        var receiptVerifierMock = new Mock<IAppleReceiptVerifier>(MockBehavior.Strict);
        receiptVerifierMock.Setup(v => v.VerifyReceiptAsync("abc", false))
            .ReturnsAsync(new VerifyReceiptResponse(null, null, null, null, null, null!, 0)
            {
                RawJson = "abc"
            });

        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = wrongRequestUrl,
                Body = new MemoryStream(Encoding.ASCII.GetBytes("{\"Receipt\":\"abc\"}"))
            },
            Response =
            {
                Body = new MemoryStream()
            },
            RequestServices = new ServiceCollection()
                .AddTransient<IAppleReceiptVerifier>(_ => receiptVerifierMock.Object)
                .BuildServiceProvider()
        };

        var requestDelegateMock = new Mock<RequestDelegate>();
        requestDelegateMock.Setup(x => x(context));
        var receiptVerifierEndpoint =
            new ReceiptVerifierEndpointMiddleware(requestDelegateMock.Object, options);

        await receiptVerifierEndpoint.InvokeAsync(context);

        requestDelegateMock.Verify(x => x(context), Times.Once);
    }

    [Fact]
    public async Task Ensure_Next_Delegate_Not_Executes_If_Request_Url_Is_Correct()
    {
        const string requestUrl = "/api/receiptInfo";

        var options = new OptionsWrapper<ReceiptVerifierMiddlewareOptions>(new ReceiptVerifierMiddlewareOptions
        {
            Path = requestUrl
        });

        var receiptVerifierMock = new Mock<IAppleReceiptVerifier>(MockBehavior.Strict);
        receiptVerifierMock.Setup(v => v.VerifyReceiptAsync("abc", false))
            .ReturnsAsync(new VerifyReceiptResponse(null, null, null, null, null, null!, 0)
            {
                RawJson = "abc"
            });


        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = requestUrl,
                Body = new MemoryStream(Encoding.ASCII.GetBytes("{\"Receipt\":\"abc\"}"))
            },
            Response =
            {
                Body = new MemoryStream()
            },
            RequestServices = new ServiceCollection()
                .AddTransient<IAppleReceiptVerifier>(_ => receiptVerifierMock.Object)
                .BuildServiceProvider()
        };

        var requestDelegateMock = new Mock<RequestDelegate>();
        requestDelegateMock.Setup(x => x(context));

        var receiptVerifierEndpoint =
            new ReceiptVerifierEndpointMiddleware(requestDelegateMock.Object, options);

        await receiptVerifierEndpoint.InvokeAsync(context);

        requestDelegateMock.Verify(x => x(context), Times.Never);
    }

    [Fact]
    public async Task Middleware_Uses_Named_Verifier()
    {
        const string verifierName = "test-verifier";
        const string requestUrl = "/api/receiptInfo";

        // Arrange
        Mock<IAppleReceiptVerifier> verifierStub = new();
        verifierStub.Setup(v => v.VerifyReceiptAsync("abc", false))
            .ReturnsAsync(new VerifyReceiptResponseStub() { RawJson = "{}" });

        Mock<IAppleReceiptVerifierResolver> resolverMock = new();
        resolverMock.Setup(r => r.Resolve(verifierName)).Returns(verifierStub.Object);
        RequestDelegate nextStub = (HttpContext ctx) => Task.CompletedTask;
        OptionsWrapper<ReceiptVerifierMiddlewareOptions> options = new(new()
        {
            Path = requestUrl,
            VerifierName = verifierName
        });

        ReceiptVerifierEndpointMiddleware middleware = new(nextStub, options);
        HttpContext ctx = new DefaultHttpContext()
        {
            Request =
            {
                Path = requestUrl,
                Body = new MemoryStream(Encoding.ASCII.GetBytes("{\"Receipt\":\"abc\"}"))
            },
            Response =
            {
                Body = new MemoryStream()
            },
            RequestServices = new ServiceCollection()
                .AddScoped<IAppleReceiptVerifierResolver>(_ => resolverMock.Object)
                .BuildServiceProvider()
        };

        // Act
        await middleware.InvokeAsync(ctx);

        // Assert
        resolverMock.Verify(r => r.Resolve(verifierName), Times.Once);
    }
}