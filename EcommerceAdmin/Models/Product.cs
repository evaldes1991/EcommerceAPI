using System;

namespace EcommerceAdmin.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    
    // Inventory & tracking added in Phase 3
    public int StockQuantity { get; set; } = 0;
    public string SKU { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductImage> Images { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
}
