| :mega: Calling for Maintainers |
|--------------|
| Because OData WebApi does not support ASP.NET Core (see https://github.com/OData/WebApi/issues/939) and I am 100% focused on new ASP.NET Core development, I don't have the capacity to maintain this project. Still, I'd love to see it live on and am seeking one or two "core" contributors / maintainers. Ideally, these would be people who have already contributed through PRs and understand the inner workings and overall design. If you're interested, please let me know by adding a comment [here](https://github.com/rbeauchamp/Swashbuckle.OData/issues/175). Thank you! |

Swashbuckle.OData
=========

[![Build status](https://ci.appveyor.com/api/projects/status/lppv9403dgwrntpa?svg=true)](https://ci.appveyor.com/project/rbeauchamp/swashbuckle-odata/)
[![Coverage Status](https://coveralls.io/repos/github/rbeauchamp/Swashbuckle.OData/badge.svg?branch=master)](https://coveralls.io/github/rbeauchamp/Swashbuckle.OData?branch=master)

Extends Swashbuckle with OData v4 support! Supports both WebApi and OData controllers! 

## Getting Started ##

Install [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle)

Install the Swashbuckle.OData NuGet package:

    Install-Package Swashbuckle.OData

In `SwaggerConfig` configure the custom provider:
```csharp
c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, GlobalConfiguration.Configuration));
```

### Include Navigation Properties in your entity swagger models ###

By default, OData does not get related entities unless you specify `$expand` on a navigation property.
Swashbuckle.OData tries to accurately reflect this behavior and therefore, by default, does not include 
navigation properties in your entity swagger models. You can override this though by specifying:
```csharp
c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, GlobalConfiguration.Configuration).Configure(odataConfig =>
                    {
                        odataConfig.IncludeNavigationProperties();
                    }));
```

### Enable caching of swagger requests ###

To enable the built-in cache functionality you must set this configuration:

```csharp
c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, GlobalConfiguration.Configuration).Configure(odataConfig =>
                    {
                        // Enable Cache for swagger doc requests
                        odataConfig.EnableSwaggerRequestCaching();
                    }));
```

### Assemblies Resolver dependency injection for OData Models ###

Configuration example to inject your own [IAssembliesResolver](https://msdn.microsoft.com/en-us/library/system.web.http.dispatcher.iassembliesresolver(v=vs.118).aspx) instead of using [DefaultAssembliesResolver](https://msdn.microsoft.com/en-us/library/system.web.http.dispatcher.defaultassembliesresolver(v=vs.118).aspx):

```csharp
c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, GlobalConfiguration.Configuration).Configure(odataConfig =>
                    {
                        //Set custom AssembliesResolver
                        odataConfig.SetAssembliesResolver(new CustomAssembliesResolver());
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
The above route resolves to an `OrdersController` (the last path segment defining the controller) and hits the `Post` action:
```csharp
[ResponseType(typeof(Order))]
public async Task<IHttpActionResult> Post([FromODataUri] int customerId, Order order)
{
  ...
}
```

### Custom property resolver ###

The following snippet demonstrates how to configure a custom property resolver, which resolves a schema's property name, instead of using a DataMemberAttribute:
```csharp
c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, GlobalConfiguration.Configuration).Configure(odataConfig =>
                    {
                        //Set custom ProperyResolver
                        odataConfig.SetProperyResolver(new DefaultProperyResolver());
                    }));
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
3. [NuGet Package Project](https://visualstudiogallery.msdn.microsoft.com/fbe9b9b8-34ae-47b5-a751-cb71a16f7e96) to generate the NuGet package.

If you submit an enhancement or bug fix, please include a unit test similar to [this](https://github.com/rbeauchamp/Swashbuckle.OData/blob/master/Swashbuckle.OData.Tests/Fixtures/FunctionTests.cs#L20) that verifies the change. Let's shoot for 100% unit test [coverage](https://coveralls.io/github/rbeauchamp/Swashbuckle.OData?branch=master) of the code base.
