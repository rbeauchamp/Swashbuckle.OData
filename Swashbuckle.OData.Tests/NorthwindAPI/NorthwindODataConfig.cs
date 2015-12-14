using System.Web.Http;
using Microsoft.Restier.EntityFramework;
using Microsoft.Restier.WebApi;
using Microsoft.Restier.WebApi.Batch;
using NorthwindAPI.Models;

namespace SwashbuckleODataSample
{
    public static class NorthwindODataConfig
    {
        public static async void Register(HttpConfiguration config, HttpServer httpServer)
        {
            await config.MapRestierRoute<DbApi<NorthwindContext>>(
                "Restier",
                "restier",
                new RestierBatchHandler(httpServer));
        }
    }
}