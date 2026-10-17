using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Threading.Tasks;
using SkirtPling.Legacy.WcfService;

namespace SkirtPling.Legacy.NetFrameworkClient
{
    /// <summary>
    /// Prints a stock reconciliation report from a .NET Framework process:
    /// shop data from the modern REST API, warehouse data from the legacy WCF service.
    /// Usage: SkirtPling.Legacy.NetFrameworkClient.exe [apiBaseUrl] [wcfAddress]
    /// </summary>
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var apiBaseUrl = args.Length > 0 ? args[0] : "http://localhost:5080";
            var wcfAddress = args.Length > 1 ? args[1] : "http://localhost:8733/LegacyWarehouse";

            ProductPage products;
            try
            {
                products = await GetProductsAsync(apiBaseUrl);
            }
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"Could not reach the REST API at {apiBaseUrl}: {ex.Message}");
                return 1;
            }

            var factory = new ChannelFactory<IWarehouseService>(new BasicHttpBinding(), new EndpointAddress(wcfAddress));
            var warehouse = factory.CreateChannel();
            try
            {
                Console.WriteLine($"{"SKU",-12}{"Product",-28}{"Shop",6}{"Warehouse",11}{"Diff",6}");
                foreach (var product in products.Items)
                {
                    var stock = warehouse.GetStock(product.Sku);
                    var onHand = stock?.OnHand.ToString() ?? "n/a";
                    var diff = stock == null ? "" : (stock.OnHand - product.StockQuantity).ToString("+0;-0;0");
                    Console.WriteLine($"{product.Sku,-12}{Truncate(product.Name, 26),-28}{product.StockQuantity,6}{onHand,11}{diff,6}");
                }
                ((IClientChannel)warehouse).Close();
                return 0;
            }
            catch (CommunicationException ex)
            {
                ((IClientChannel)warehouse).Abort();
                Console.Error.WriteLine($"Could not reach the WCF service at {wcfAddress}: {ex.Message}");
                return 1;
            }
            finally
            {
                factory.Abort();
            }
        }

        private static async Task<ProductPage> GetProductsAsync(string apiBaseUrl)
        {
            using (var http = new HttpClient { BaseAddress = new Uri(apiBaseUrl) })
            using (var response = await http.GetAsync("/api/products?pageSize=50&sort=name"))
            {
                response.EnsureSuccessStatusCode();
                using (Stream body = await response.Content.ReadAsStreamAsync())
                {
                    return (ProductPage)new DataContractJsonSerializer(typeof(ProductPage)).ReadObject(body);
                }
            }
        }

        private static string Truncate(string value, int length) =>
            value.Length <= length ? value : value.Substring(0, length - 1) + "…";
    }
}
