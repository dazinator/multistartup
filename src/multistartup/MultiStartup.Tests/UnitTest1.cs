namespace MultiStartup.Tests
{
    using MultiStartup.Identify;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

            IServiceCollection services = new ServiceCollection();
            services.AddIdentifyRequest(Selectors.Instance.HostNoPort(), Matchers.Instance.Literal());

            // **
            services.Configure<MappingOptions<int>>((options) => options.Add(new Mapping<int>()
            {
                Key = 1,
                Patterns = new string[] { "**" }
            }));
        }
    }
}
