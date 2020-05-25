namespace MultiStartup.Identify
{
    using System.Collections.Generic;

    public interface IMappingMatcherProvider<TKey>
    {
        IEnumerable<MappingMatcher<TKey>> GetMatchers(MappingOptions<TKey> options);
    }
}
