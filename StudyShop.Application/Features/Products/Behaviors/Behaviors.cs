namespace StudyShop.Application.Features.Products.Behaviors;

public interface ICacheableRequest<TResponse>
{
    string GetCacheKey();
}

public static class CacheKeyHelper
{
    public static string GetProductsListKey(string? search, int skip, int take)
        => $"products:list:{search}:{skip}:{take}";
}


