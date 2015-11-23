Swashbuckle.OData
=========

[![Build status](https://ci.appveyor.com/api/projects/status/lppv9403dgwrntpa?svg=true)](https://ci.appveyor.com/project/rbeauchamp/swashbuckle-odata/)
[![NuGet](https://img.shields.io/nuget/v/Swashbuckle.OData.svg?style=flat)](https://www.nuget.org/packages/Swashbuckle.OData/)
[![Issue Stats](http://www.issuestats.com/github/rbeauchamp/Swashbuckle.OData/badge/issue)](http://www.issuestats.com/github/rbeauchamp/Swashbuckle.OData)

Extends Swashbuckle with WebApi OData v4 support!

Implements a custom Swagger Provider that converts an Entity Data Model to a Swagger Document.

### <a href="http://swashbuckleodata.azurewebsites.net/swagger/" target="_blank">Try it out!</a> ###

## Getting Started ##

Install [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle)

Install the Swashbuckle.OData NuGet package:

    Install-Package Swashbuckle.OData

Update `SwaggerConfig` to accept an Entity Data Model:
```csharp
//[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Swashbuckle.OData
{
    public class SwaggerConfig
    {
        public static void Register(IEdmModel edmModel)
        {
```
In `SwaggerConfig` configure the custom provider:
```csharp
// Wrap the default SwaggerGenerator with additional behavior (e.g. caching) or provide an
// alternative implementation for ISwaggerProvider with the CustomProvider option.
//
c.CustomProvider(defaultProvider => new ODataSwaggerProvider(edmModel));
```
When you build your Entity Data Model, pass it to `SwaggerConfig` during registration. For example:
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
