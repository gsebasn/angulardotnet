namespace StudyShop.Domain.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedUtc { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}


