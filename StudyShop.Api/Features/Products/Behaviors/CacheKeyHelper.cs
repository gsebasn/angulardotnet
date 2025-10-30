using StudyShop.Api.Features.Products.Queries;

namespace StudyShop.Api.Features.Products.Behaviors;

/// <summary>
/// Helper class for generating consistent cache keys for product queries.
/// </summary>
public static class CacheKeyHelper
{
    private const string ProductsListPrefix = "products:list:";
    private const string ProductsByIdPrefix = "products:id:";

    public static string GetProductsListKey(string? search = null, int skip = 0, int take = 100)
    {
        return $"{ProductsListPrefix}{search ?? "all"}:skip:{skip}:take:{take}";
    }

    public static string GetProductByIdKey(int productId)
    {
        return $"{ProductsByIdPrefix}{productId}";
    }

    public static string GetProductsListPrefix()
    {
        return ProductsListPrefix;
    }

    public static string GetProductsByIdPrefix()
    {
        return ProductsByIdPrefix;
    }
}
