using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Flurl;
using Swashbuckle.OData.Tests.WebHost;
using SwashbuckleODataSample;

namespace Swashbuckle.OData.Tests
{
    public class HttpClientUtils
    {
        public static HttpClient GetHttpClient()
        {
            var client =  new HttpClient
            {
                BaseAddress = new Uri(TestWebApiStartup.BaseAddress.AppendPathSegment(ODataConfig.ODataRoutePrefix)),
                Timeout = TimeSpan.FromMilliseconds(5 * 60 * 1000)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType.ApplicationJson));

            return client;
        }
    }
}