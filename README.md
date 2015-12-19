Swashbuckle.OData
=========

[![Build status](https://ci.appveyor.com/api/projects/status/lppv9403dgwrntpa?svg=true)](https://ci.appveyor.com/project/rbeauchamp/swashbuckle-odata/)
[![Issue Stats](http://www.issuestats.com/github/rbeauchamp/Swashbuckle.OData/badge/pr)](http://www.issuestats.com/github/rbeauchamp/Swashbuckle.OData)
[![Issue Stats](http://www.issuestats.com/github/rbeauchamp/Swashbuckle.OData/badge/issue)](http://www.issuestats.com/github/rbeauchamp/Swashbuckle.OData)

Extends Swashbuckle with OData v4 support!

### <a href="http://swashbuckleodata.azurewebsites.net/swagger/" target="_blank">Try it out!</a> ###

## Getting Started ##

Install [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle)

Install the Swashbuckle.OData NuGet package:

    Install-Package Swashbuckle.OData

In `SwaggerConfig` configure the custom provider:
```csharp
c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c));
```

### Custom Routes  ###

The following snippet demonstrates how to configure a custom OData route such that it will appear in the Swagger UI:
```csharp
// Let's say you map a custom OData route that doesn't follow the typical conventions
var customODataRoute = config.MapODataServiceRoute("CustomODataRoute", ODataRoutePrefix, GetModel(), batchHandler: null, pathHandler: new DefaultODataPathHandler(), routingConventions: myCustomConventions);

// Then describe your route to Swashbuckle.OData so that it will appear in the Swagger UI
config.AddCustomSwaggerRoute(customODataRoute, "/Customers({Id})/Orders")
    .Operation(HttpMethod.Post)
    // The name of the parameter as it appears in the path
    .PathParameter<int>("Id")
    // The name of the parameter as it appears in the controller action
    .BodyParameter<Order>("order");
```
The above route resolves to an `OrderController` action of:
```csharp
[ResponseType(typeof(Order))]
public async Task<IHttpActionResult> Post([FromODataUri] int customerId, Order order)
{
  ...
}
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
        c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, () => httpConfiguration));
    })
    .EnableSwaggerUi();
```
