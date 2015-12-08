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
            return new ODataParameterDescriptor(parameter.name, GetType(parameter), !parameter.required.Value)
            {
                Configuration = actionDescriptor.ControllerDescriptor.Configuration,
                ActionDescriptor = actionDescriptor
            };
        }

        private static Type GetType(PartialSchema queryParameter)
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
                            throw new Exception($"Could not determine .NET type for parameter type {type} and format 'null'");
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
                case "guid":
                    return typeof(Guid);
                case "binary":
                    return typeof(byte[]);
                default:
                    throw new Exception($"Could not determine .NET type for parameter type {type} and format {format}");
            }
        }
    }
}