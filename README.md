# 📝 What is ReceiptVerifierMiddlewareEndpoint?

ReceiptVerifierMiddlewareEndpoint is a library used to add an endpoint to verify and get information about receipt from Apple. It uses [AppleReceiptVerifier.NET](https://github.com/alexalok/AppleReceiptVerifier.NET) to get information from Apple.

## Installation

Use the package manager [NuGet](https://www.nuget.org/packages/ReceiptVerifierMiddlewareEndpoint) to install library.

```bash
NuGet\Install-Package ReceiptVerifierMiddlewareEndpoint -Version 1.0.1
```

## ⚙️  Configuration
Before beginning please make sure you injected AppleReceiptVerifier.NET to your project. Otherwise you will get an exception.

Firstly use AddReceiptVerifierEndpointMiddleware method and pass path. You will be able to get receipt info by using that path.
```csharp
services.AddReceiptVerifierEndpointMiddleware(op =>
{
    op.Path = "/api/Subscriptions/receiptInfo";
});
```
Then call UseReceiptVerifierEndpointMiddleware.
```csharp
app.UseReceiptVerifierEndpointMiddleware();
```

## ℹ️ Usage
Execute GET request on path you set in configuration. Body of request must contain valid receipt.
```json
{
    "receipt" : "very long base64encoded string"
}
```