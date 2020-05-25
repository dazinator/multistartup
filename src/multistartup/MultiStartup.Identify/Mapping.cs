namespace MultiStartup.Identify
{
    public class Mapping<TKey>
    {       
        public TKey Key { get; set; }
        public string[] Patterns { get; set; }
    }
}
