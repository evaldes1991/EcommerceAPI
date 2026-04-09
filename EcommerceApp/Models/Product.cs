namespace EcommerceApp.Models;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public required string ImageUrl { get; set; }
    public List<ProductImage> Images { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
}
