using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace SkirtPling.Legacy.WcfService
{
    /// <summary>
    /// The legacy warehouse contract. The modern API keeps a copy of this contract in
    /// SkirtPling.DAL/External/LegacyWarehouseClient.cs; namespaces and names must stay identical.
    /// </summary>
    [ServiceContract(Namespace = WarehouseContract.Namespace)]
    public interface IWarehouseService
    {
        /// <summary>Returns the physical stock count for a SKU, or null if the warehouse does not stock it.</summary>
        [OperationContract]
        WarehouseStock GetStock(string sku);
    }

    [DataContract(Namespace = WarehouseContract.Namespace)]
    public class WarehouseStock
    {
        [DataMember] public string Sku { get; set; }
        [DataMember] public int OnHand { get; set; }
        [DataMember] public string WarehouseCode { get; set; }
        [DataMember] public DateTime LastCountedAt { get; set; }
    }

    public static class WarehouseContract
    {
        public const string Namespace = "http://skirtpling.example/legacy/warehouse";
    }
}
