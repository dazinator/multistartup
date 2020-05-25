namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        public const string HttpContextItemName = "Identifier";

        public static TKey GetMappedKey<TKey>(this HttpContext httpContext)
        {
            if(httpContext.Items.TryGetValue(HttpContextItemName, out var identifier))
            {
                return (TKey)identifier;
            }

            return default;
        }

        internal static void SetMappedKey<TKey>(this HttpContext httpContext, TKey value) => httpContext.Items.Add(HttpContextItemName, value);
    }
}
