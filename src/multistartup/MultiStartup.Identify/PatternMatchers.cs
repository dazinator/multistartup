namespace MultiStartup.Identify
{
    public static class PatternOptionsExtensions
    {
#pragma warning disable IDE0060 // Unused parameter needed for sugar extension method.
        public static CreatePatternMatcher Literal(this Matchers options) => new CreatePatternMatcher(GetLiteralMatcher);
#pragma warning restore IDE0060 // Remove unused parameter

        private static IPatternMatcher GetLiteralMatcher(string pattern) =>
            // authorityUriBuilder.Host           
            new LiteralPatternMatcher(pattern);

    }
}
