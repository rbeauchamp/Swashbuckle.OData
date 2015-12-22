using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json.Linq;
using Owin;

namespace Swashbuckle.OData.Tests.Containment
{
    public class Program
    {
        private static readonly string _baseAddress = "http://localhost:12345";
        private static readonly HttpClient _httpClient = new HttpClient();

        public static void Main(string[] args)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            using (WebApp.Start(_baseAddress, Configuration))
            {
                Console.WriteLine("Listening on " + _baseAddress);

                // Query the metadata
                QueryMetadata();

                // Query Accounts and expand their PayoutPI
                ExpandPayoutPI();

                // Query PayinPIs directly
                QueryPayinPIs();

                // Create a new payin PI
                AddPayinPI();

                // Update the payout pi, add a new one if it doesn't exist
                AddOrUpdatePayoutPI();

                // Delete a payin PI
                DeletePayinPI();

                // Call a funciton bound to a collection of contained entities:
                GetPayinPIsCountWhoseNameContainsGivenString();

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        public static void Configuration(IAppBuilder builder)
        {
            var config = new HttpConfiguration();
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            config.MapODataServiceRoute("OData", "odata", ODataModels.GetModel());
            builder.UseWebApi(config);
        }

        public static void QueryMetadata()
        {
            var requestUri = _baseAddress + "/odata/$metadata";
            using (var response = _httpClient.GetAsync(requestUri).Result)
            {
                var metadata = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                Console.WriteLine("\nThe metadata is \n {0}", metadata);
            }
        }

        public static void ExpandPayoutPI()
        {
            var requestUri = _baseAddress + "/odata/Accounts?$expand=PayoutPI";
            using (var response = _httpClient.GetAsync(requestUri).Result)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                Console.WriteLine("\nQuery Accounts and expand their PayoutPI:");
                Console.WriteLine(JObject.Parse(result));
            }
        }

        public static void QueryPayinPIs()
        {
            var requestUri = _baseAddress + "/odata/Accounts(100)/PayinPIs";

            using (var response = _httpClient.GetAsync(requestUri).Result)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                Console.WriteLine("\nThe PayinPIs are:");
                Console.WriteLine(JObject.Parse(result));
            }
        }

        public static void AddPayinPI()
        {
            ResetDatasource();
            var requestUri = _baseAddress + "/odata/Accounts(100)/PayinPIs";

            var postContent = JObject.Parse(@"{'PaymentInstrumentID':1,'FriendlyName':'Credit Card'}");
            using (var response = _httpClient.PostAsJsonAsync(requestUri, postContent).Result)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                Console.WriteLine("\nThe newly added payout PI is:");
                Console.WriteLine(JObject.Parse(result));
            }
        }

        public static void AddOrUpdatePayoutPI()
        {
            ResetDatasource();
            var requestUri = _baseAddress + "/odata/Accounts(100)/PayoutPI";
            var content = JObject.Parse(@"{'PaymentInstrumentID':1,'FriendlyName':'Direct Debit'}");

            using (var response = _httpClient.PutAsJsonAsync(requestUri, content).Result)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                Console.WriteLine("\nThe payout PI is:");
                Console.WriteLine(JObject.Parse(result));
            }
        }

        public static void DeletePayinPI()
        {
            ResetDatasource();
            var requestUri = _baseAddress + "/odata/Accounts(100)/PayinPIs(101)";
            using (var response = _httpClient.DeleteAsync(requestUri).Result)
            {
                response.EnsureSuccessStatusCode();
                Console.WriteLine("\nThe PayinPI with id 101 is deleted.");
            }
        }

        public static void GetPayinPIsCountWhoseNameContainsGivenString()
        {
            ResetDatasource();
            var requestUri = _baseAddress + "/odata/Accounts(100)/PayinPIs/ContainmentExample.GetCount(NameContains='10')";
            using (var response = _httpClient.GetAsync(requestUri).Result)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                var count = (int) JObject.Parse(result)["value"];

                Console.WriteLine("\nThe count of payin PIs whose name contains '10' is:");
                Console.WriteLine(count);
            }
        }

        private static void ResetDatasource()
        {
            var uriReset = _baseAddress + "/odata/ResetDataSource";
            var response = _httpClient.PostAsync(uriReset, null).Result;
        }
    }
}