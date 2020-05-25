namespace MultiStartup.Identify.DotNetGlob
{
    using DotNet.Globbing;

    public class GlobPatternMatcher : IPatternMatcher
    {
        private readonly Glob _glob;

        public GlobPatternMatcher(string globPattern, GlobOptions options) => _glob = Glob.Parse(globPattern, options);
        public bool IsMatch(string testValue) => _glob.IsMatch(testValue);
    }
}
