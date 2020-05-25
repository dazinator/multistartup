namespace MultiStartup.Identify
{
    using System.Collections.Generic;

    public class MappingOptions<TKey>
    {
        public MappingOptions() => Mappings = new List<Mapping<TKey>>();
        public List<Mapping<TKey>> Mappings { get; set; }

        public MappingOptions<TKey> Add(Mapping<TKey> mapping)
        {
            Mappings.Add(mapping);
            return this;
        }
    }

    public delegate IPatternMatcher CreatePatternMatcher(string pattern);
}
