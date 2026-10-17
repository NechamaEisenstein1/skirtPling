using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace SkirtPling.Legacy.WcfService
{
    /// <summary>
    /// Self-hosts the WCF service in a console window (no IIS needed).
    /// Usage: SkirtPling.Legacy.WcfService.exe [baseAddress]
    /// </summary>
    internal static class Program
    {
        private const string DefaultAddress = "http://localhost:8733/LegacyWarehouse";

        private static void Main(string[] args)
        {
            var address = args.Length > 0 ? args[0] : DefaultAddress;

            using (var host = new ServiceHost(typeof(WarehouseService), new Uri(address)))
            {
                // BasicHttpBinding (SOAP 1.1 over HTTP) is the binding modern .NET clients support best.
                host.AddServiceEndpoint(typeof(IWarehouseService), new BasicHttpBinding(), string.Empty);
                // Publishes the WSDL at <address>?wsdl so tools like dotnet-svcutil can read the contract.
                host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });

                host.Open();
                Console.WriteLine($"Legacy warehouse service listening on {address}");
                Console.WriteLine("Press Enter to stop.");
                Console.ReadLine();
            }
        }
    }
}
