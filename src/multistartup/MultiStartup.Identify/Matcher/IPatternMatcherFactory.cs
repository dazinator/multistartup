namespace MultiStartup.Identify
{
    public interface IPatternMatcherFactory<TKey>
    {
        IPatternMatcher Create(string pattern);
    }   
}
