using System;
using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using Swashbuckle.Swagger;

namespace Swashbuckle.OData.Descriptions
{
    internal class RestierParameterDescriptor : HttpParameterDescriptor
    {
        public RestierParameterDescriptor(Parameter parameter)
        {
            Contract.Requires(parameter != null);

            DefaultValue = null;
            Prefix = null;
            ParameterName = parameter.name;
            IsOptional = !parameter.required ?? false;
            ParameterType = parameter.GetClrType();
        }

        public override string ParameterName { get; }

        public override Type ParameterType { get; }

        public override bool IsOptional { get; }

        public override object DefaultValue { get; }

        public override string Prefix { get; }
    }
}