namespace MultiStartup.Identify
{
    using System;

    public class LiteralPatternMatcher : DelegatePatternMatcher
    {
        public LiteralPatternMatcher(string patternAsLiteral) : base(patternAsLiteral, (a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase))
        {
        }
    }


}
