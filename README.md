Swashbuckle.OData 1.1.0
=========

[![Build status](https://ci.appveyor.com/api/projects/status/lppv9403dgwrntpa?svg=true)](https://ci.appveyor.com/project/rbeauchamp/swashbuckle-odata)

Extends Swashbuckle with WebApi OData v4 support!

Implements a custom Swagger Provider that converts an Edm Model to a Swagger Document.

__<a href="http://swashbuckleodata.azurewebsites.net/swagger/" target="_blank">Try it out!</a>__

## Getting Started ##

Install [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle)

Install the Swashbuckle.OData NuGet package:

    Install-Package Swashbuckle.OData

Update your Swagger Config to accept an Edm Model:
```csharp
//[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Swashbuckle.OData
{
    public class SwaggerConfig
    {
        public static void Register(IEdmModel edmModel)
        {
```

In your Swagger Config configure the custom provider:
```csharp
// Wrap the default SwaggerGenerator with additional behavior (e.g. caching) or provide an
// alternative implementation for ISwaggerProvider with the CustomProvider option.
//
c.CustomProvider(defaultProvider => new ODataSwaggerProvider(edmModel));
```

When you build your OData Edm Model, pass it to Swagger Config during registration. For example:
```csharp
public static void Register(HttpConfiguration config)
{
    var builder = new ODataConventionModelBuilder();
    var edmModel = builder.GetEdmModel();
    config.MapODataServiceRoute("odata", "odata", edmModel);

    SwaggerConfig.Register(edmModel);
}
```

Note that, currently, the <code>ODataSwaggerProvider</code> assumes an ODataServiceRoute of "odata".
