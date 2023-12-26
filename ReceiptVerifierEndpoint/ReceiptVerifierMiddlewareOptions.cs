namespace ReceiptVerifierMiddlewareEndpoint;

public class ReceiptVerifierMiddlewareOptions
{
    public string Path { get; set; } = null!;
    public string? VerifierName { get; set; } = null!;
}