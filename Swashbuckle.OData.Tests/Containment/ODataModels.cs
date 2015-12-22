using System.Web.OData.Builder;
using Containment;
using Microsoft.OData.Edm;

namespace Swashbuckle.OData.Tests.Containment
{
    public class ODataModels
    {
        public static IEdmModel GetModel()
        {
            var builder = new ODataConventionModelBuilder();
            var paymentInstrumentType = builder.EntityType<PaymentInstrument>();

            builder.EntitySet<Account>("Accounts");

            var functionConfiguration = paymentInstrumentType.Collection.Function("GetCount");
            functionConfiguration.Parameter<string>("NameContains");
            functionConfiguration.Returns<int>();

            builder.Action("ResetDataSource");

            builder.Namespace = typeof (Account).Namespace;

            return builder.GetEdmModel();
        }
    }
}