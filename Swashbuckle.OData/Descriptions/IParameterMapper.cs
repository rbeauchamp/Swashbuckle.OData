using System;
using System.Linq;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData
{
    public interface IParameterMapper
    {
        HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor);
    }

    public class MapByParameterName : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetParameters()
                .SingleOrDefault(descriptor => string.Equals(descriptor.ParameterName, parameter.name, StringComparison.CurrentCultureIgnoreCase));
        }
    }

    public class MapByDescription : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            // Maybe the parameter is a key parameter, e.g., where Id in the URI path maps to a parameter named 'key'
            if (parameter.description.StartsWith("key:"))
            {
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

    public class MapByIndex : IParameterMapper
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

    public class MapToDefault : IParameterMapper
    {
        public HttpParameterDescriptor Map(Parameter parameter, int index, HttpActionDescriptor actionDescriptor)
        {
            return new ODataParameterDescriptor(parameter.name, GetType(parameter), !parameter.required.Value)
            {
                Configuration = actionDescriptor.ControllerDescriptor.Configuration,
                ActionDescriptor = actionDescriptor
            };
        }

        private static Type GetType(Parameter queryParameter)
        {
            var type = queryParameter.type;
            var format = queryParameter.format;

            switch (format)
            {
                case null:
                    switch (type)
                    {
                        case "string":
                            return typeof(string);
                        case "boolean":
                            return typeof(bool);
                        default:
                            throw new Exception(string.Format("Could not determine .NET type for parameter type {0} and format {1}", type, "null"));
                    }
                case "int32":
                    return typeof(int);
                case "int64":
                    return typeof(long);
                case "byte":
                    return typeof(byte);
                case "date":
                    return typeof(DateTime);
                case "date-time":
                    return typeof(DateTimeOffset);
                case "double":
                    return typeof(double);
                case "float":
                    return typeof(float);
                default:
                    throw new Exception(string.Format("Could not determine .NET type for parameter type {0} and format {1}", type, format));
            }
        }
    }
}