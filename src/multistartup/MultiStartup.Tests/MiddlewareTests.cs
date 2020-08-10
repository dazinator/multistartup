namespace IdentifyRequest.Tests
{
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Http;
    using MultiStartup.Identify;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Builder;
    using System;
    using System.Linq.Expressions;

    public class MiddlewareTests
    {
        [Fact]
        public async Task Adds_HttpContextItem_For_Key_When_MatchedRequest()
        {
            using var host = await new HostBuilder()
               .ConfigureWebHost(webBuilder => webBuilder
                       .UseTestServer()
                       .ConfigureServices(services =>
                       {
                           services.AddLogging();

                           TenantPartitionServiceCollectionExtensions.AddPartitionType<TenantPartition, int>(services, options =>
                           {
                               options.ConfigureKeyMappings((a) => a.Mappings.Add(new Mapping<int>()
                               {
                                   Key = 1,
                                   Patterns = new string[] { "localhost" }
                               }))
                              .ConfigureMatching(Selectors.Instance.HostHeaderNoPort(), Matchers.Instance.Literal());

                           });
                       })
                       .Configure(app => app.UseMiddleware<IdentifyMiddleware<int>>()))
               .StartAsync();

            var server = host.GetTestServer();
            server.BaseAddress = new Uri("http://localhost:5000/");

            var context = await server.SendAsync(c => c.Request.Method = HttpMethods.Get);

            var key = context.GetMappedKey<int>();
            Assert.Equal(1, key);
        }
    }

    public class TenantPartition
    {

    }

    public static class TenantPartitionServiceCollectionExtensions
    {
        public static IServiceCollection AddPartitionType<TPartition, TKey>(IServiceCollection services, Action<PartitionTypeOptionsBuilder<TPartition, TKey>> configurePartitionType)
        {
            //services.
            var builder = new PartitionTypeOptionsBuilder<TPartition, TKey>(services);
            configurePartitionType(builder);
            return services;
        }
    }
    public class PartitionTypeOptionsBuilder<TPartition, TKey>
    {
        private readonly IServiceCollection _services;

        public PartitionTypeOptionsBuilder(IServiceCollection services) => _services = services;

        public PartitionTypeOptionsBuilder<TPartition, TKey> ConfigureKeyMappings(Action<MappingOptions<TKey>> configureMappings)
        {
            _services.Configure<MappingOptions<TKey>>(configureMappings);
            return this;
        }

        public PartitionTypeOptionsBuilder<TPartition, TKey> ConfigureMatching(SelectValue selector, CreatePatternMatcher patternMatcher)
        {
            _services.AddIdentifyRequest(selector, patternMatcher);
            return this;
        }
    }
}
