using System;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class ODataParameterDescriptor : HttpParameterDescriptor
    {
        public ODataParameterDescriptor(string parameterName, Type parameterType, bool isOptional)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
            IsOptional = isOptional;
        }

        public override string ParameterName { get; }

        public override Type ParameterType { get; }

        public override bool IsOptional { get; }


    }

    internal class ODataActionParameterDescriptor : ODataParameterDescriptor
    {
        public ODataActionParameterDescriptor(string parameterName, Type parameterType, bool isOptional, Schema schema) : base(parameterName, parameterType, isOptional)
        {
            Contract.Requires(schema != null);

            Schema = schema;
        }

        public Schema Schema { get; private set; }
    }
}