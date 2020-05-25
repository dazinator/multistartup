## Features

Progressive startup, with multitenancy in mind, made easy, for your asp.net core platform.

- A flexible mapper to partition different types of requests into logical groups, each group is identified by an assigned key, for example in a multitenant application, you can map requests for your different tenants, to their own tenant ID key's.
    - The mapping configuration is updateable whilst the application is running! It uses the `IOptions` system and so can be configured via `IConfiguration` providers such as JsonFile etc.
- Enables a clean mechanism for you to add a progressive startup routine to your asp.net core platform.
    - When your platform starts up for the first time, you want a nice way for it to start in Platform Setup mode - so you can display your platform setup UI and only load the services needed to perform setup.
    - After that perhaps you want it to restart in "Tenant Setup" mode. For example to initialise the current tenant's settings, database connection string etc.
    - After that you want the application to load in Tenant Setup Complete mode. Setup has been complete so the application should load normally for the current tenant, i.e register the Tenant's db context, and MVC or Blazor SPA app etc to be served.

ASP.NET Core gives you a single `startup` class, and this is a lot of logic to place in that class, with complicated if checks, forks of the middleware pipeline, and it may be confusing to know which services to use where.

This library solves that problem.

1. It provides a middleware to categorise an incoming request, and assign it a Key. The mapping is configurable and you can update it whilst the application is running, for exmaple to add in new tenants without a restart. The mapping maps the request to a Key, the Key is just stored in HttpContext.Items for use later. If you don't want to use the inbuilt mapper, just replace it with whatever your own logic is to identify the current request with key value indicating the tenant / category. As long as you set the Key (in HttpContext.Items) at the start of the request, then everything else downstream that this library provides will continue to work.
2. It allows you to map different `startup` classes to different keys. The first time a request is received for a particular key, the startup class will be resolved, and executed.
3. It allows you to map multiple different `startup` classes to the same key, but condition which one is currently "enabled", for example, based on whether Tenant Setup has been completed.

The basic idea is that the first time you browse to tenant idenfied with key 1, this library will look at the different startup classes you have mapped to that key, and take the one that is enabled with the highest precedence, and use that to initialise the services, and middleware pipeline for the current partition identified by that key. In future requests mapped to the same key, the services, and middleware pipeline will be restored and the request will flow through it, essentially partitioning your asp.net core application by the key specified. So the startup class is only executed once to initialise that partition, or again when the partition with the same key is restarted,

It provides a soft restart mechanism (in memory) so that the partition can be reloaded without stopping the application. 
It lets you configure the startup classes used to initialisse the partition with the key 1. You can map multiple startups, in order of precedence, and each with an optional runtime condition (predicate or class if you need dependencies injected) that will be evaluated to work out it the startup class is "enabled". If its not enabled, it proceeds to check the next one until it finds one or no-ops. You can therefore create a condition on the TenantSetupStartup class to only be enabled when no tenant-settings.json exists (or whatever logic you want). This startup class can just register your services and endpoints toserver your setup experience. Once your tenant setup page is complete, you can save this settings file and restart the partition (not the applicaiton, just an operation in memory). The next time you browse to the smae tenant, this library will need to initialise that partition again, this time the enabled condition for TenantSetupStartup will fail as the tenant-settings.josn file does exist, and therefore it will try the next Startup class assigned to the key 1, in this case that would be your normal "TenantStartup" - this startup class can load your configured application and endpoints.

This basically lets you partition your application into multliple modes of operation in a clean way. It's more general concept than just multitenance, although it was designed with that need in mind. You could also just use it for a non multitenant application, that requires the ability to serve a setup experience first.

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
