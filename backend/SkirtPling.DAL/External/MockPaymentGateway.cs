namespace SkirtPling.DAL.External;

/// <summary>
/// A sandbox gateway that behaves like a provider's test mode: known test tokens approve or decline,
/// anything else is rejected. Replace with a real provider client (Stripe, Tranzila, ...) in production.
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    public const string ApprovedToken = "tok_test_approved";
    public const string DeclinedToken = "tok_test_declined";

    public Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken ct)
    {
        var result = request.PaymentToken switch
        {
            ApprovedToken => new PaymentResult(true, $"mock_{Guid.NewGuid():N}", null),
            DeclinedToken => new PaymentResult(false, null, "The card was declined."),
            _ => new PaymentResult(false, null, "Unknown payment token.")
        };
        return Task.FromResult(result);
    }
}
