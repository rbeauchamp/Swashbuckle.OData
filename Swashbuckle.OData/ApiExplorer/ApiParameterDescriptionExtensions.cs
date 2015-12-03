// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

namespace Swashbuckle.OData.ApiExplorer
{
    public static class ApiParameterDescriptionExtensions
    {
        public static IEnumerable<PropertyInfo> GetBindableProperties(this HttpParameterDescriptor httpParameterDescriptor)
        {
            return GetBindableProperties(httpParameterDescriptor.ParameterType);
        }

        public static bool CanConvertPropertiesFromString(this ApiParameterDescription apiParameterDescription)
        {
            return apiParameterDescription.ParameterDescriptor.GetBindableProperties().All(p => TypeHelper.CanConvertFromString(p.PropertyType));
        }

        public static IEnumerable<PropertyInfo> GetBindableProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null);
        }
    }
}