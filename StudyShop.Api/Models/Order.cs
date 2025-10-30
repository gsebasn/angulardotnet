namespace StudyShop.Api.Models;

/// <summary>
/// Represents a customer order.
/// </summary>
public class Order
{
    public int Id { get; set; }
    
    /// <summary>
    /// Unique order number (e.g., "ORD-2024-001")
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// List of items in this order
    /// </summary>
    public List<OrderItem> Items { get; set; } = new();
    
    /// <summary>
    /// Total price of the order (computed from items)
    /// </summary>
    public decimal Total => Items.Sum(item => item.UnitPrice * item.Quantity);
    
    /// <summary>
    /// When the order was created
    /// </summary>
    public DateTime CreatedUtc { get; set; }
}

/// <summary>
/// Represents a single line item in an order.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    
    /// <summary>
    /// The product being ordered
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// Navigation property to the product
    /// </summary>
    public Product Product { get; set; } = null!;
    
    /// <summary>
    /// Quantity ordered (must be >= 1)
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Price per unit at time of order
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Parent order
    /// </summary>
    public int OrderId { get; set; }
    
    /// <summary>
    /// Navigation property to the order
    /// </summary>
    public Order Order { get; set; } = null!;
}

