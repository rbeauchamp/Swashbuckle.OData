using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Flurl;

namespace Swashbuckle.OData.Tests
{
    public class HttpClientUtils
    {
        public const string BaseAddress = "http://localhost:8347/";

        public static HttpClient GetHttpClient(string baseAddress, string routePrefix = null)
        {
            var client =  new HttpClient
            {
                BaseAddress = string.IsNullOrWhiteSpace(routePrefix) ? new Uri(baseAddress) : new Uri(baseAddress.AppendPathSegment(routePrefix)),
                Timeout = TimeSpan.FromMilliseconds(5 * 60 * 1000)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType.ApplicationJson));

            return client;
        }
    }
}