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
      

        /// <summary>
        /// Create instance of middleware.
        /// </summary>
        /// <param name="next">The next middleware delegate in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="optionsMonitor">Options monitor used to get access to current mapping options.</param>
        /// <param name="provider">Provides matchers, that encapsulate how the request value is matched to mappings e.e literal matches, or "fuzzy" matches etc.</param>
        /// <param name="selectValue">Selects the value from the request that will be mapped to an identification value.</param>
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
                // Whenever mappings options change, we re-build the lazy, so our matchers can re-compile against the new set of mappings.
                _logger.LogInformation("Change detected for mapping options, reloading.");
                SetLazy(a);
            });
        }

        private void SetLazy(MappingOptions<TKey> options = null)
        {
            var opts = options ?? _optionsMonitor.CurrentValue;
            _lazyTenantMatchers = new Lazy<IEnumerable<MappingMatcher<TKey>>>(() => _provider.GetMatchers(opts));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var matchers = _lazyTenantMatchers.Value;     
            var valueToMap = _selectValue(httpContext);

            // TODO: Could cache the identification value for valueToMap, then check the cache first? This would
            // save evaluating the matchers on repeat requests.
            // Although if using fuzzy matching and lots and lots of different reuqest values, this might not be so desirable?
            // e.g ** -> 1 would match any value, so if browsing on 10000 urls, this cache would grow to 10000 items all mapped to the same value,
            // and would grow infinitely if an attacker browsed on a large volume of matching (but unique) urls.
            // So to mitigate this, let's use a cache that:
            //  - Has a max size
            //  - Expires least recently used items
            //  - Expires all items when mappings options are changed..
            //  - When to Compact() ?

            //TODO: If we didn't find in the cache then evalutate the matchers, to get the value.
            // Evaluate each matcher, if the matcher can match this request value, then we have identified the request.
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
