Swashbuckle.OData v2.0.0-alpha
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

In `SwaggerConfig` configure the custom provider:
```csharp
// Wrap the default SwaggerGenerator with additional behavior (e.g. caching) or provide an
// alternative implementation for ISwaggerProvider with the CustomProvider option.
//
c.CustomProvider(defaultProvider => new ODataSwaggerProvider());
```

### OWIN  ###

If your service is hosted using OWIN middleware, configure the custom provider as follows:
```csharp
httpConfiguration
    .EnableSwagger(c =>
    {
        // Use "SingleApiVersion" to describe a single version API. Swagger 2.0 includes an "Info" object to
        // hold additional metadata for an API. Version and title are required but you can also provide
        // additional fields by chaining methods off SingleApiVersion.
        //
        c.SingleApiVersion("v1", "A title for your API");

        // Wrap the default SwaggerGenerator with additional behavior (e.g. caching) or provide an
        // alternative implementation for ISwaggerProvider with the CustomProvider option.
        //
        c.CustomProvider(defaultProvider => new ODataSwaggerProvider(() => httpConfiguration));
    })
    .EnableSwaggerUi();
```

### Upgrading to Swashbuckle.OData 2.0 ###

To simplify configuration, this version of Swashbuckle.OData leverages .NET 4.5. Previous versions were compiled against .NET 4.0.
