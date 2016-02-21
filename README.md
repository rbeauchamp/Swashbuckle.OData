Swashbuckle.OData
=========

[![Build status](https://ci.appveyor.com/api/projects/status/lppv9403dgwrntpa?svg=true)](https://ci.appveyor.com/project/rbeauchamp/swashbuckle-odata/)
[![Coverage Status](https://coveralls.io/repos/github/rbeauchamp/Swashbuckle.OData/badge.svg?branch=master)](https://coveralls.io/github/rbeauchamp/Swashbuckle.OData?branch=master)
[![Issue Stats](http://www.issuestats.com/github/rbeauchamp/Swashbuckle.OData/badge/pr)](http://www.issuestats.com/github/rbeauchamp/Swashbuckle.OData)
[![Issue Stats](http://www.issuestats.com/github/rbeauchamp/Swashbuckle.OData/badge/issue)](http://www.issuestats.com/github/rbeauchamp/Swashbuckle.OData)

Extends Swashbuckle with OData v4 support! Supports both WebApi and OData controllers! 

### <a href="http://swashbuckleodata.azurewebsites.net/swagger/" target="_blank">Try it out!</a> ###

## Getting Started ##

Install [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle)

Install the Swashbuckle.OData NuGet package:

    Install-Package Swashbuckle.OData

In `SwaggerConfig` configure the custom provider:
```csharp
c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c));
```

### Include Navigation Properties in your entity swagger models ###

By default, OData does not get related entities unless you specify `$expand` on a navigation property.
Swashbuckle.OData tries to accurately reflect this behavior and therefore, by default, does not include 
navigation properties in your entity swagger models. You can override this though by specifying:
```csharp
c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c).Configure(odataConfig =>
                    {
                        odataConfig.IncludeNavigationProperties();
                    }));
```

### Custom Swagger Routes ###

The following snippet demonstrates how to configure a custom swagger route such that it will appear in the Swagger UI:
```csharp
// Let's say you map a custom OData route that doesn't follow the OData conventions 
// and where the target controller action doesn't have an [ODataRoute] attribute
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

#### RESTier ####

By default, Swashbuckle.OData only displays RESTier routes for top-level entity types. You can describe and display additional routes, for related types, in the Swagger UI by configuring custom swagger routes. For example from the Northwind model, to display a route that queries an Order (a related type) for a Customer (a top-level entity type), configure the following:

```csharp
var restierRoute = await config.MapRestierRoute<DbApi<NorthwindContext>>("RESTierRoute", "restier", new RestierBatchHandler(server));

config.AddCustomSwaggerRoute(restierRoute, "/Customers({CustomerId})/Orders({OrderId})")
    .Operation(HttpMethod.Get)
    .PathParameter<string>("CustomerId")
    .PathParameter<int>("OrderId");
```

### Route prefixes that have parameters ###

The follow snippet demonstrates how to configure route prefixes that have parameters:

```csharp
// For example, if you have a route prefix with a parameter "tenantId" of type long
var odataRoute = config.MapODataServiceRoute("odata", "odata/{tenantId}", builder.GetEdmModel());

// Then add the following route constraint so that Swashbuckle.OData knows the parameter type.
// If you don't add this line then the parameter will be assumed to be of type string.
odataRoute.Constraints.Add("tenantId", new LongRouteConstraint());
```
Swashbuckle.OData supports the following route constraints:

| Parameter Type | Route Constraint          |
|----------------|---------------------------|
| `bool`         | `BoolRouteConstraint`     |
| `DateTime`     | `DateTimeRouteConstraint` |
| `decimal`      | `DecimalRouteConstraint`  |
| `double`       | `DoubleRouteConstraint`   |
| `float`        | `FloatRouteConstraint`    |
| `Guid`         | `GuidRouteConstraint`     |
| `int`          | `IntRouteConstraint`      |
| `long`         | `LongRouteConstraint`     |


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
        c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, httpConfiguration));
    })
    .EnableSwaggerUi();
```

### Development  ###

You'll need:

1. Visual Studio 2015
2. [Code Contracts](https://visualstudiogallery.msdn.microsoft.com/1ec7db13-3363-46c9-851f-1ce455f66970)

If you submit an enhancement or bug fix, please include a unit test similar to [this](https://github.com/rbeauchamp/Swashbuckle.OData/blob/master/Swashbuckle.OData.Tests/Fixtures/FunctionTests.cs#L20) that verifies the change. Let's shoot for 100% unit test [coverage](https://coveralls.io/github/rbeauchamp/Swashbuckle.OData?branch=master) of the code base.
