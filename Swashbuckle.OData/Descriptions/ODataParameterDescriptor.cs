using System;
using System.Web.Http.Controllers;

namespace Swashbuckle.OData
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
}