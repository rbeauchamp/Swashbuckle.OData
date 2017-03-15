using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace Swashbuckle.OData.Descriptions
{
    internal static class HttpActionDescriptorExtensions
    {
        public static ResponseDescription CreateResponseDescription(this HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(actionDescriptor != null);
                        
            var responseType = actionDescriptor.GetCustomAttributes<Swagger.Annotations.SwaggerResponseAttribute>()?
                .Select(attribute => attribute.Type).FirstOrDefault();
            if (responseType == null)            
                responseType = actionDescriptor.GetCustomAttributes<ResponseTypeAttribute>()?
                    .Select(attribute => attribute.ResponseType).FirstOrDefault();
            
            return new ResponseDescription
            {
                DeclaredType = actionDescriptor.ReturnType,
                ResponseType = responseType,
                Documentation = actionDescriptor.GetApiResponseDocumentation()
            };
        }

        private static string GetApiResponseDocumentation(this HttpActionDescriptor actionDescriptor)
        {
            Contract.Requires(actionDescriptor != null);

            Contract.Assume(actionDescriptor.Configuration != null);

            var documentationProvider = actionDescriptor.Configuration.Services.GetDocumentationProvider();
            return documentationProvider?.GetResponseDocumentation(actionDescriptor);
        }
    }
}