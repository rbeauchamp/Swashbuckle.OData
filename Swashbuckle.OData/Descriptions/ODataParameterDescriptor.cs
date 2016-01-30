using System;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataParameterDescriptor : HttpParameterDescriptor
    {
        public ODataParameterDescriptor(string parameterName, Type parameterType, bool isOptional, HttpParameterDescriptor reflectedHttpParameterDescriptor)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
            IsOptional = isOptional;
            ReflectedHttpParameterDescriptor = reflectedHttpParameterDescriptor;
        }

        public override string ParameterName { get; }

        public override Type ParameterType { get; }

        public override bool IsOptional { get; }

        public HttpParameterDescriptor ReflectedHttpParameterDescriptor { get; }
    }

    internal class ODataActionParameterDescriptor : ODataParameterDescriptor
    {
        public ODataActionParameterDescriptor(string parameterName, Type parameterType, bool isOptional, Schema schema, HttpParameterDescriptor reflectedHttpParameterDescriptor) : base(parameterName, parameterType, isOptional, reflectedHttpParameterDescriptor)
        {
            Contract.Requires(schema != null);

            Schema = schema;
        }

        public Schema Schema { get; private set; }
    }
}