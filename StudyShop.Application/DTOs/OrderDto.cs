namespace StudyShop.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public decimal Total { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public record OrderItemDto
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}


