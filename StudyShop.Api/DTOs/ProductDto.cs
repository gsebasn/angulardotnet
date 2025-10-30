namespace StudyShop.Api.DTOs;

/// <summary>
/// Data Transfer Object for creating/updating a product.
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedUtc { get; set; }
}

/// <summary>
/// DTO for creating a new product (excluding Id and CreatedUtc).
/// </summary>
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

/// <summary>
/// DTO for updating an existing product (all fields optional).
/// </summary>
public class UpdateProductDto
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
}

