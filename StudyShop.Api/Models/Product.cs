namespace StudyShop.Api.Models;

/// <summary>
/// Represents a product in the shop.
/// </summary>
public class Product
{
    public int Id { get; set; }
    
    /// <summary>
    /// Product name (required, 2-100 characters)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Price in currency units (must be >= 0)
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Available stock quantity (must be >= 0)
    /// </summary>
    public int Stock { get; set; }
    
    /// <summary>
    /// When this product was created
    /// </summary>
    public DateTime CreatedUtc { get; set; }
    
    /// <summary>
    /// Navigation property for order items referencing this product
    /// </summary>
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

