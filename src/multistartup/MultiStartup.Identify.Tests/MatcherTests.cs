namespace MultiStartup.Identify.Tests
{
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Http;
    using System;
    using MultiStartup.Identify;

    public class MatcherTests
    {
        [Fact]
        public void Can_Match_Literal_HostNoPort()
        {
            var services = new ServiceCollection();
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

            AddIdentifyRequest(services, Selectors.Instance.HostNoPort(), Matchers.Instance.Literal());

            var sp = services.BuildServiceProvider();

            AssertMatchRequest(sp, "localhost", 5000);
        }

        [Fact]
        public void Can_Match_Glob_HostNoPort()
        {
            var services = new ServiceCollection();
            services.Configure<MappingOptions<int>>((options) =>
            {
                options.Add(new Mapping<int>()
                {
                    Key = 1,
                    Patterns = new string[] { "*.localhost" }
                });
            });


            AddIdentifyRequest(services, Selectors.Instance.HostNoPort(), Matchers.Instance.Glob());

            var sp = services.BuildServiceProvider();

            AssertMatchRequest(sp, "foo.localhost", 5000);
        }


        private void AssertMatchRequest(ServiceProvider sp, string hostName, int port, Action<string, MappingMatcher<int>> onMatched = null)
        {
            var mappingProvider = sp.GetRequiredService<IMappingMatcherProvider<int>>();
            var mappingOptions = sp.GetRequiredService<IOptions<MappingOptions<int>>>();
            var matchers = mappingProvider.GetMatchers(mappingOptions.Value);
            var valueSelector = sp.GetRequiredService<SelectValue>();

            var httpContext = new DefaultHttpContext();
            var request = httpContext.Request;
            request.Scheme = "http";
            request.Host = new HostString(hostName, port);

            var valueToMap = valueSelector(httpContext);

            foreach (var item in matchers)
            {
                if (item.IsMatch(valueToMap))
                {
                    onMatched?.Invoke(valueToMap, item);
                    return;
                }
            }

            throw new Exception("No match");

        }

        public IServiceCollection AddIdentifyRequest(IServiceCollection services, SelectValue selectValueStrategy, CreatePatternMatcher usePatternMatcher)
        {
            services.AddIdentifyRequest(selectValueStrategy, usePatternMatcher);
            return services;
        }
    }
}
