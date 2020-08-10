namespace MultiStartup.Tests
{
    using MultiStartup.Identify;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

            IServiceCollection services = new ServiceCollection();
            services.AddIdentifyRequest(Selectors.Instance.HostHeaderNoPort(), Matchers.Instance.Literal());

            // **
            services.Configure<MappingOptions<int>>((options) => options.Add(new Mapping<int>()
            {
                Key = 1,
                Patterns = new string[] { "**" }
            }));
        }

        [Fact]
        public async Task Test_Lru_Cache()
        {

            // not expecting to keep more than a 100 urls. (i.e 100 active tenants)/
            var cache = new LRUCache<string, int>(100);
            var maxRequests = 1000;

            var tasks = new List<Task>();

            for (var i = 0; i < maxRequests; i++)
            {
                var requestValue = i.ToString();
                tasks.Add(GetOrAddCacheItem(cache, requestValue, i));
            }

            await Task.WhenAll(tasks);
            // lru cache should only have kept 100 last used.
            for (var i = 900; i < 999; i++)
            {
                var val = cache.Get(i.ToString());
                Assert.NotEqual(0, val);
            }

        }

        private async Task GetOrAddCacheItem(LRUCache<string, int> cache, string requestValue, int id)
        {
            var cached = cache.Get(requestValue);
            if (cached == default)
            {
                cached = id;
                cache.Add(requestValue, cached);
            }
        }
    }

}
