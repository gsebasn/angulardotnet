using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace StudyShop.Api.Features.Products.Behaviors;

/// <summary>
/// MediatR pipeline behavior that caches responses for requests that explicitly opt-in via ICacheableRequest.
/// </summary>
public class QueryCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<QueryCachingBehavior<TRequest, TResponse>> _logger;

    public QueryCachingBehavior(IMemoryCache cache, ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only cache requests that implement ICacheableRequest
        if (request is not ICacheableRequest<TResponse> cacheable)
        {
            return await next();
        }

        var cacheKey = cacheable.GetCacheKey();

        if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse) && cachedResponse is not null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResponse;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

        var response = await next();

        if (response is not null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheable.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = cacheable.SlidingExpiration,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, response, options);
            _logger.LogDebug("Cached response for key: {CacheKey}", cacheKey);
        }

        return response;
    }
}
