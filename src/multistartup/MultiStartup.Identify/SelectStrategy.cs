namespace MultiStartup.Identify
{
    using Microsoft.AspNetCore.Http;

    public static class SelectValueStrategy
    {
#pragma warning disable IDE0060 // Param needed for sugar extension method
        public static SelectValue HostNoPort(this Selectors options) => new SelectValue(GetHostNoPort);
#pragma warning restore IDE0060 // Remove unused parameter

        private static string GetHostNoPort(HttpContext httpContext) =>
            // authorityUriBuilder.Host           
            httpContext?.Request?.GetUri()?.Host;
    }

}
