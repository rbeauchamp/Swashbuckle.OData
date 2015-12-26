using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Swashbuckle.OData.Tests
{
    public class HttpClientUtils
    {
        public const string BaseAddress = "http://localhost:8347/";

        public static HttpClient GetHttpClient(string baseAddress)
        {
            var client =  new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromMilliseconds(5 * 60 * 1000)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType.ApplicationJson));

            return client;
        }
    }
}