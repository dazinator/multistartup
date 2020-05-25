namespace MultiStartup.Identify
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The default pattern matcher does not cater for patterns at all! Strings must match based on an ordinal match ignoring casing.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class MappingMatcherProvider<TKey> : IMappingMatcherProvider<TKey>
    {
        private readonly IPatternMatcherFactory<TKey> _patternMatcherFactory;

        public MappingMatcherProvider(IPatternMatcherFactory<TKey> patternMatcherFactory) => _patternMatcherFactory = patternMatcherFactory;
        public virtual IEnumerable<MappingMatcher<TKey>> GetMatchers(MappingOptions<TKey> options)
        {
            var matchers = new List<MappingMatcher<TKey>>();
            foreach (var item in options?.Mappings)
            {
                var key = item.Key;
                var patterns = item.Patterns.Select(a => _patternMatcherFactory.Create(a));
                var tenantPatterMatcher = new MappingMatcher<TKey>(item, patterns);
                matchers.Add(tenantPatterMatcher);
            }

            return matchers.AsEnumerable();
        }
    }
}
