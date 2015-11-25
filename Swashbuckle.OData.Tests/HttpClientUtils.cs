using System;
using System.Net.Http;
using Flurl;
using Swashbuckle.OData.Tests.WebHost;
using SwashbuckleODataSample;

namespace Swashbuckle.OData.Tests
{
    public class HttpClientUtils
    {
        public static HttpClient GetHttpClient()
        {
            return new HttpClient
            {
                BaseAddress = new Uri(TestWebApiStartup.BaseAddress.AppendPathSegment(WebApiConfig.ODataRoutePrefix)),
                Timeout = TimeSpan.FromMilliseconds(5 * 60 * 1000)
            };
        }
    }
}