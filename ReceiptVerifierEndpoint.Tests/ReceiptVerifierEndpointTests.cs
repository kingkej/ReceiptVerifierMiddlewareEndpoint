using System.Text;
using System.Text.Json;
using AppleReceiptVerifierNET;
using AppleReceiptVerifierNET.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using ReceiptVerifierMiddlewareEndpoint;
using ReceiptVerifierMiddlewareEndpoint.Middlewares;
using Environment = AppleReceiptVerifierNET.Models.Environment;

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
            }
        };
        
        var receiptVerifierMock = new Mock<IAppleReceiptVerifier>(MockBehavior.Strict);
        receiptVerifierMock.Setup(v => v.VerifyReceiptAsync("abc", false))
            .ReturnsAsync(new VerifyReceiptResponse(null, null, null, null, null, null!, 0)
            {
                RawJson = "abc"
            });
        var receiptVerifierEndpoint =
            new ReceiptVerifierEndpointMiddleware(null!, receiptVerifierMock.Object, options);
        
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
            }
        };
        
        var receiptVerifierMock = new Mock<IAppleReceiptVerifier>(MockBehavior.Strict);
        receiptVerifierMock.Setup(v => v.VerifyReceiptAsync("abc", false))
            .ReturnsAsync(new VerifyReceiptResponse(null, null, null, null, null, null!, 0)
            {
                RawJson = "abc"
            });
        var requestDelegateMock = new Mock<RequestDelegate>();
        requestDelegateMock.Setup(x => x(context));
        var receiptVerifierEndpoint = 
            new ReceiptVerifierEndpointMiddleware(requestDelegateMock.Object, receiptVerifierMock.Object, options);

        await receiptVerifierEndpoint.InvokeAsync(context);
        
        requestDelegateMock.Verify(x=>x(context), Times.Once);
    }
    
    [Fact]
    public async Task Ensure_Next_Delegate_Not_Executes_If_Request_Url_Is_Correct()
    {
        const string requestUrl = "/api/receiptInfo";

        var options = new OptionsWrapper<ReceiptVerifierMiddlewareOptions>(new ReceiptVerifierMiddlewareOptions
        {
            Path = requestUrl
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
            }
        };
        
        var receiptVerifierMock = new Mock<IAppleReceiptVerifier>(MockBehavior.Strict);
        receiptVerifierMock.Setup(v => v.VerifyReceiptAsync("abc", false))
            .ReturnsAsync(new VerifyReceiptResponse(null, null, null, null, null, null!, 0)
            {
                RawJson = "abc"
            });
        var requestDelegateMock = new Mock<RequestDelegate>();
        requestDelegateMock.Setup(x => x(context));
        var receiptVerifierEndpoint = 
            new ReceiptVerifierEndpointMiddleware(requestDelegateMock.Object, receiptVerifierMock.Object, options);

        await receiptVerifierEndpoint.InvokeAsync(context);
        
        requestDelegateMock.Verify(x=>x(context), Times.Never);
    }
}