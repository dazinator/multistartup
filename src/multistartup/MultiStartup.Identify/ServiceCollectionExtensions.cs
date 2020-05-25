namespace Microsoft.Extensions.DependencyInjection
{
    using MultiStartup.Identify;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentifyRequest(this IServiceCollection services, SelectValue selector, CreatePatternMatcher patternMatcher)
        {
            services.AddSingleton<IMappingMatcherProvider<int>, MappingMatcherProvider<int>>();
            services.AddSingleton<IPatternMatcherFactory<int>>(new DelegatePatternMatcherFactory<int>(patternMatcher));

            services.AddSingleton<SelectValue>(selector);
            return services;
        }
    }
}
