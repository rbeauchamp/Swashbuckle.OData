using System;
using System.Linq;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    internal interface IParameterMapper
    {
        HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor);
    }

    internal class MapByParameterName : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetParameters()
                .SingleOrDefault(descriptor => string.Equals(descriptor.ParameterName, parameter.name, StringComparison.CurrentCultureIgnoreCase));
        }
    }

    internal class MapByDescription : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            // Maybe the parameter is a key parameter, e.g., where Id in the URI path maps to a parameter named 'key'
            if (parameter.description.StartsWith("key:"))
            {
                var paramerters = actionDescriptor.GetParameters();
                var parameterDescriptor = actionDescriptor.GetParameters().SingleOrDefault(descriptor => descriptor.ParameterName == "key");
                if (parameterDescriptor != null)
                {
                    // Need to assign the correct name expected by OData
                    return new ODataParameterDescriptor(parameter.name, parameterDescriptor.ParameterType, parameterDescriptor.IsOptional)
                    {
                        Configuration = actionDescriptor.ControllerDescriptor.Configuration,
                        ActionDescriptor = actionDescriptor,
                        ParameterBinderAttribute = parameterDescriptor.ParameterBinderAttribute
                    };
                }
            }
            return null;
        }
    }

    internal class MapRestierParameter : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor.ControllerDescriptor.ControllerName == "Restier")
            {
                return new RestierParameterDescriptor(parameter)
                {
                    Configuration = actionDescriptor.ControllerDescriptor.Configuration,
                };
            }
            return null;
        }
    }

    internal class MapByIndex : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            if (parameter.@in != "query" && index < actionDescriptor.GetParameters().Count)
            {
                var parameterDescriptor = actionDescriptor.GetParameters()[index];
                if (parameterDescriptor != null)
                {
                    // Need to assign the correct name expected by OData
                    return new ODataParameterDescriptor(parameter.name, parameterDescriptor.ParameterType, parameterDescriptor.IsOptional)
                    {
                        Configuration = actionDescriptor.ControllerDescriptor.Configuration,
                        ActionDescriptor = actionDescriptor,
                        ParameterBinderAttribute = parameterDescriptor.ParameterBinderAttribute
                    };
                }
            }
            return null;
        }
    }

    internal class MapToDefault : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            return new ODataParameterDescriptor(parameter.name, parameter.GetClrType(), !parameter.required.Value)
            {
                Configuration = actionDescriptor.ControllerDescriptor.Configuration,
                ActionDescriptor = actionDescriptor
            };
        }
    }
}