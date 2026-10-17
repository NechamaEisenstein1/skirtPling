using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.Extensions.Options;

namespace SkirtPling.DAL.External;

public class LegacyWarehouseOptions
{
    public const string SectionName = "LegacyWarehouse";

    /// <summary>URL of the .NET Framework WCF service. Empty means the integration is switched off.</summary>
    public string Endpoint { get; set; } = string.Empty;
}

/// <summary>The legacy service could not be reached or failed. Hides WCF exception types from callers.</summary>
public class LegacyWarehouseUnavailableException(string message, Exception inner) : Exception(message, inner);

public record LegacyStockLevel(string Sku, int OnHand, string WarehouseCode, DateTime LastCountedAt);

/// <summary>Reads physical stock counts from the legacy warehouse system (a WCF service).</summary>
public interface ILegacyWarehouseClient
{
    bool IsConfigured { get; }
    Task<LegacyStockLevel?> GetStockLevelAsync(string sku, CancellationToken ct);
}

public class LegacyWarehouseClient(IOptions<LegacyWarehouseOptions> options) : ILegacyWarehouseClient
{
    private readonly string endpoint = options.Value.Endpoint;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(endpoint);

    public async Task<LegacyStockLevel?> GetStockLevelAsync(string sku, CancellationToken ct)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("The legacy warehouse endpoint is not configured.");

        var binding = new BasicHttpBinding { SendTimeout = TimeSpan.FromSeconds(10), OpenTimeout = TimeSpan.FromSeconds(5) };
        var factory = new ChannelFactory<IWarehouseService>(binding, new EndpointAddress(endpoint));
        var channel = factory.CreateChannel();
        try
        {
            var stock = await channel.GetStockAsync(sku).WaitAsync(ct);
            ((IClientChannel)channel).Close();
            return stock is null ? null : new LegacyStockLevel(stock.Sku, stock.OnHand, stock.WarehouseCode, stock.LastCountedAt);
        }
        catch (Exception ex) when (ex is CommunicationException or TimeoutException)
        {
            ((IClientChannel)channel).Abort();
            throw new LegacyWarehouseUnavailableException("The legacy warehouse service did not respond.", ex);
        }
        finally
        {
            factory.Abort();
        }
    }
}

// The WCF contract below mirrors LegacyIntegration/WcfService. The two sides run on different runtimes
// (.NET Framework 4.8 vs .NET 10), so the contract is duplicated by design rather than shared as a package.

[ServiceContract(Namespace = WarehouseContract.Namespace)]
public interface IWarehouseService
{
    [OperationContract(Action = WarehouseContract.Namespace + "/IWarehouseService/GetStock",
                       ReplyAction = WarehouseContract.Namespace + "/IWarehouseService/GetStockResponse")]
    Task<WarehouseStock?> GetStockAsync(string sku);
}

[DataContract(Namespace = WarehouseContract.Namespace)]
public class WarehouseStock
{
    [DataMember] public string Sku { get; set; } = string.Empty;
    [DataMember] public int OnHand { get; set; }
    [DataMember] public string WarehouseCode { get; set; } = string.Empty;
    [DataMember] public DateTime LastCountedAt { get; set; }
}

internal static class WarehouseContract
{
    public const string Namespace = "http://skirtpling.example/legacy/warehouse";
}
