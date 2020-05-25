namespace MultiStartup.Identify
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    

    public class IdentifyMiddleware<TKey>: IDisposable
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IdentifyMiddleware<TKey>> _logger;
        private readonly IOptionsMonitor<MappingOptions<TKey>> _optionsMonitor;
        private readonly IMappingMatcherProvider<TKey> _provider;
        private readonly SelectValue _selectValue;
        private Lazy<IEnumerable<MappingMatcher<TKey>>> _lazyTenantMatchers;

        private readonly IDisposable _optionsOnChangeToken;
        private bool _disposedValue;
      

        public IdentifyMiddleware(RequestDelegate next,
            ILogger<IdentifyMiddleware<TKey>> logger,
            IOptionsMonitor<MappingOptions<TKey>> optionsMonitor,
            IMappingMatcherProvider<TKey> provider,
            SelectValue selectValue)
        {
            _next = next;
            _logger = logger;
            _optionsMonitor = optionsMonitor;
            _provider = provider;
            _selectValue = selectValue;

            SetLazy(optionsMonitor.CurrentValue);
            _optionsOnChangeToken = optionsMonitor.OnChange((a) =>
            {
                _logger.LogInformation("Change detected for mapping options, reloading.");
                SetLazy(a);
            });
        }

        private void SetLazy(MappingOptions<TKey> options = null)
        {
            var opts = options ?? _optionsMonitor.CurrentValue;
            _lazyTenantMatchers = new Lazy<IEnumerable<MappingMatcher<TKey>>>(() => _provider.GetMatchers(opts));
        }

        // IMyScopedService is injected into Invoke ,mjk9llllllllllllll
        public async Task Invoke(HttpContext httpContext)
        {
            var matchers = _lazyTenantMatchers.Value;     
            var valueToMap = _selectValue(httpContext);

            foreach (var item in matchers)
            {
                if (item.IsMatch(valueToMap))
                {
                    httpContext.SetMappedKey<TKey>(item.Mapping.Key);
                    break;
                }
            }

            await _next?.Invoke(httpContext);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_optionsOnChangeToken != null)
                    {
                        _optionsOnChangeToken.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        public void Dispose() =>
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
    }
}
