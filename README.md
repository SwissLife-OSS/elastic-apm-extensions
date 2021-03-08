Elastic APM extensions for multiple .NET libraries.
## Features

- Messaging
  - [X] [MassTransit](#masstrasit)
  - [ ] Azure Service Bus

- GraphQL
  - [X] [HotChocolate](#hotchocolate)

- Storage
  - [ ] Azure Blob Storage

### MassTransit
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

### HotChocolate
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
## Community

This project has adopted the code of conduct defined by the [Contributor Covenant](https://contributor-covenant.org/)
to clarify expected behavior in our community. For more information, see the [Swiss Life OSS Code of Conduct](https://swisslife-oss.github.io/coc).
