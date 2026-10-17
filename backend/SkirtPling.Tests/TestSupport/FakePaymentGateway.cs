using SkirtPling.DAL.External;

namespace SkirtPling.Tests.TestSupport;

public class FakePaymentGateway(bool approve) : IPaymentGateway
{
    public List<PaymentRequest> Requests { get; } = [];

    public Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken ct)
    {
        Requests.Add(request);
        return Task.FromResult(approve
            ? new PaymentResult(true, "fake-tx-1", null)
            : new PaymentResult(false, null, "Declined by test"));
    }
}
