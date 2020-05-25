## Features

Progressive startup, with multitenancy in mind, made easy, for your asp.net core platform.

- A flexible mapper to assign different types of requests a unique key, for example based on hostname or any other request value. 
    - The mapping configuration is updateable whilst the application is running! It uses the `IOptions` system and so can be configured via `IConfiguration` providers such as JsonFile etc.
- Enables a clean mechanism for you to add a progressive startup routine to your asp.net core platform.
    - When your platform starts up for the first time, you want a nice way for it to start in Platform Setup mode - so you can display your platform setup UI and only load the services needed to perform setup.
    - After that perhaps you want it to restart in "Tenant Setup" mode. For example to initialise the current tenant's settings, database connection string etc.
    - After that you want the application to load in Tenant Setup Complete mode. Setup has been complete so the application should load normally for the current tenant, i.e register the Tenant's db context, and MVC or Blazor SPA app etc to be served.

ASP.NET Core gives you a single `startup` class, and this is a lot of logic to place in that class, with complicated if checks, forks of the middleware pipeline, and it may be confusing to know which services to use where.

This library solves that problem.

1. It provides a middleware to assign an incoming request a Key based on a configurable mapping of the current request, that you can update at runtime (i.e to map in new tenants etc). This can be used to ensure each tenant, even if they have multiple URL's, is mapped at the start of the request to the specific Key that identifies them. This Key is just added to HttpContext.Items for use later. If you don't want to use the inbuilt mapper, just replace it with whatever your own logic is to identify the current tenant, as long as you set the Key (in HttpContext.Items) at the start of the request, then everything else downstream that this library provides will continue to work.
2. It allows you to map `startup` classes to keys. The first time a request is received for a particular key, the startup class will be resolved and executed.
3. It allows you to map multiple `startup` classes to the same key, but condition which one is currently enabled, for example, based on whether Tenant Setup has been completed.

// TODO: Finish this doc.

## Map inbound request to an Key with a mapping thats updateable at runtime.

```csharp

public class Startup
{

    public void ConfigureServices(IServiceCollection services)
    {        
        services.AddLogging();
        // Configure how you want want to map incoming requests to a particular keys.
        services.Configure<MappingOptions<int>>((mappings) =>
        {
            mappings.Add(new Mapping<int>()
            {
                Key = 1,
                Patterns = new string[] { "localhost" }
            })
            .Add(new Mapping<int>()
            {
                Key = 2,
                Patterns = new string[] { "foo.bar.com", "foo.bar.co.uk" }
            });
        });       

        // Match on the Hostname to the patterns above, literally. Note glob patterns are also available.
        services.AddIdentifyRequest(Selectors.Instance.HostNoPort(), Matchers.Instance.Literal());
	}

    public void Configure(IApplicationBuilder appBuilder)
    {
       // This middleware will match incoming requests based on the mapping patterns set above to the Key.
       // The key is then stored in HttpContext.Items. 
       // So basically this allows you to identify what type of request you are dealing with - i.e perhaps its for a particular tenant etc.
       appBuilder.Use<IdentifyMiddleware<int>>();
       appBuilder.Run((httpContext)=>{  var key = httpContext.GetMappedKey<int>(); // will be 1 for localhost.  })

	}

}
```

The above is a simple way to assign a Key to an incoming request based on a configurable mapping.
For example you could map each tenant's hostnames to their key.

The middleware uses `IOptionsMonitor` to listen for changes to the mappings at runtime,
and update its local cached mappings values, for efficiency.

The mappings themselves are configured via the normal `IOptions` system, so can be backed by `IConfiguration` providers such as Json etc.
This means you can update the config for the mappings whilst the application is running.