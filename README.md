Elastic APM extensions for multiple .NET libraries.
## Features

  - [X] [MassTransit](#masstrasit)
  - [X] [HotChocolate](#hotchocolate)

## MassTransit
### Usage
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .UseElasticApm(
            // Add additional diagnostic subscribers e.g. AspNetCore, Http, etc
            new MassTransitDiagnosticsSubscriber());
```

The following events from the [MassTransit DiagnosticSource](https://masstransit-project.com/advanced/monitoring/diagnostic-source.html) are instrumented:
- `Transport.Send` 
- `Transport.Receive`
### Options
By default the Elastic APM send/receive label will be as follow:
- "Send {label}" where label is [`SendContext.DestinationAddress.AbsolutePath`](https://github.com/MassTransit/MassTransit/blob/5e2a416384f005c392ead139f5c4af34511c56db/src/MassTransit/SendContext.cs#L31)
- "Receive {label}" where label is [`ReceiveContext.InputAddress.AbsolutePath`](https://github.com/MassTransit/MassTransit/blob/5e2a416384f005c392ead139f5c4af34511c56db/src/MassTransit/ReceiveContext.cs#L24)

This can be changed when creating the `MassTransitDiagnosticsSubscriber` by providing a different label.
```csharp
new MassTransitDiagnosticsSubscriber(o => 
  o.ReceiveLabel = context => context.Host.AbsolutePath)
```
or if you return `null`, the default label will be used.
```csharp
new MassTransitDiagnosticsSubscriber(o => 
  o.ReceiveLabel = context => 
    if (context is RabbitMqReceiveContext rabbitMqContext)
      ? rabbitMqContext.Exchange
      : default;)
```
The same can be used also for `SendLabel`
## HotChocolate
### Usage
HotChocolate by default is not emitting diagnostic events, but has the infrastructure to instrument each request.
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add your services

        // Add HotChocolate GraphQL Server
        services
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddObservability(); // Register instrumentation for Elastic APM
    }
}
```
### Options
Elastic APM Transaction can be enriched by registering a delegate on configure parameter. In this way you can add custom data to the transaction.
```csharp
services
    .AddGraphQLServer()
    .AddObservability(o => 
        o.Enrich = (transaction, operationDetails) =>
        {
            transaction.SetLabel("GraphQLResult", operationDetails.HasFailed);
            transaction.SetLabel("Department", Environment.GetEnvironmentVariable("DEPARTMENT"));
        });
```

## Community

This project has adopted the code of conduct defined by the [Contributor Covenant](https://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [Swiss Life OSS Code of Conduct](https://swisslife-oss.github.io/coc).
