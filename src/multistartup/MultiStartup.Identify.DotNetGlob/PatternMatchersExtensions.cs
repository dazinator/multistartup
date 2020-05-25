namespace MultiStartup.Identify
{
    using DotNet.Globbing;
    using MultiStartup.Identify.DotNetGlob;

    public static class PatternOptionsExtensions
    {
        private static readonly GlobOptions Options = new GlobOptions() { Evaluation = new EvaluationOptions() { CaseInsensitive = true } };

#pragma warning disable IDE0060 // Unused parameter needed for sugar extension method.
        public static CreatePatternMatcher Glob(this Matchers matchingOptions) => new CreatePatternMatcher(GetGlobPatternMatcher);
#pragma warning restore IDE0060 // Remove unused parameter

        private static IPatternMatcher GetGlobPatternMatcher(string pattern) => new GlobPatternMatcher(pattern, Options);
    }
}
