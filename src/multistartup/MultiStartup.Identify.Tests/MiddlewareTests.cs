namespace IdentifyRequest.Tests
{
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Http;
    using MultiStartup.Identify;
    using System.Threading.Tasks;

    public class MiddlewareTests
    {
        [Fact]
        public async Task Adds_HttpContextItem_For_Key_When_MatchedRequest()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            // Configure how we want to map incoming requests to particular keys.
            services.Configure<MappingOptions<int>>((a) => a.Mappings.Add(new Mapping<int>()
            {
                Key = 1,
                Patterns = new string[] { "localhost" }
            }));

            services.AddIdentifyRequest(Selectors.Instance.HostNoPort(), Matchers.Instance.Literal());

            var sp = services.BuildServiceProvider();
            await AssertMappedKey(1, sp, "localhost", 5000);
        }

        private async Task AssertMappedKey(int expectedKey, ServiceProvider sp, string hostName, int port)
        {
            var httpContext = new DefaultHttpContext();
            var request = httpContext.Request;
            request.Scheme = "http";
            request.Host = new HostString(hostName, port);

            using var scope = sp.CreateScope();

            var requestDelegate = new RequestDelegate(async (h) =>
            {

            });

            var sut = ActivatorUtilities.CreateInstance<IdentifyMiddleware<int>>(sp, requestDelegate);           

            await sut.Invoke(httpContext);

            var key = httpContext.GetMappedKey<int>();
            Assert.Equal(expectedKey, key);
        }
    }
}
