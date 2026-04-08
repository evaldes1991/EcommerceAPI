using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceAPI.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; }
    
    public string ImageUrl { get; set; } = string.Empty;
    
    [JsonIgnore]
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
