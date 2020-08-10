namespace MultiStartup.Identify
{
    using Microsoft.AspNetCore.Http;

    public static class SelectValueStrategy
    {
#pragma warning disable IDE0060 // Param needed for sugar extension method
        public static SelectValue HostHeaderNoPort(this Selectors options) => new SelectValue(GetHostHeaderNoPort);
        public static SelectValue HostHeader(this Selectors options) => new SelectValue(GetHostHeader);

#pragma warning restore IDE0060 // Remove unused parameter

        private static string GetHostHeaderNoPort(HttpContext httpContext) =>
            // authorityUriBuilder.Host           
            httpContext?.Request?.Host.Host;

        private static string GetHostHeader(HttpContext httpContext) =>
           // authorityUriBuilder.Host           
           httpContext?.Request?.Host.Value;
    }

}
