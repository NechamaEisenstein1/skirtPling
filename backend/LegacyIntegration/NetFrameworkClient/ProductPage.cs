using System.Runtime.Serialization;

namespace SkirtPling.Legacy.NetFrameworkClient
{
    // Minimal shapes of the REST API's JSON responses, read with DataContractJsonSerializer
    // (built into .NET Framework, so no NuGet packages are needed).

    [DataContract]
    public class ProductPage
    {
        [DataMember(Name = "items")] public ProductItem[] Items { get; set; }
        [DataMember(Name = "totalCount")] public int TotalCount { get; set; }
    }

    [DataContract]
    public class ProductItem
    {
        [DataMember(Name = "sku")] public string Sku { get; set; }
        [DataMember(Name = "name")] public string Name { get; set; }
        [DataMember(Name = "price")] public decimal Price { get; set; }
        [DataMember(Name = "stockQuantity")] public int StockQuantity { get; set; }
    }
}
