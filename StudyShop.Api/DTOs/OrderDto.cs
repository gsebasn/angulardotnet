namespace StudyShop.Api.DTOs;

/// <summary>
/// Data Transfer Object for order item.
/// </summary>
public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Data Transfer Object for creating an order.
/// </summary>
public class CreateOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Data Transfer Object for returning order details.
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
    public DateTime CreatedUtc { get; set; }
}

