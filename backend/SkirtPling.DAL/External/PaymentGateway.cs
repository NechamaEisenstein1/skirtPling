namespace SkirtPling.DAL.External;

public record PaymentRequest(string OrderNumber, decimal Amount, string Currency, string PaymentToken);

public record PaymentResult(bool Succeeded, string? TransactionId, string? FailureReason);

/// <summary>
/// The boundary to a payment provider. The shop only ever passes a provider token, never card numbers.
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken ct);
}
