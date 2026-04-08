using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.Models;

namespace EcommerceAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] bool includeInactive = false, [FromQuery] int? categoryId = null)
    {
        IQueryable<Product> query = _context.Products
            .Include(p => p.Categories)
            .Include(p => p.Images);
        
        if (!includeInactive) 
            query = query.Where(p => p.IsActive);
            
        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(p => p.Categories.Any(c => c.Id == categoryId.Value));
        
        var products = await query.ToListAsync();
        
        // Map current category lists to ID lists for the client
        foreach (var p in products)
        {
            p.CategoryIds = p.Categories.Select(c => c.Id).ToList();
        }
        
        return products;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Categories)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();
        
        product.CategoryIds = product.Categories.Select(c => c.Id).ToList();
        return product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> PostProduct(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        if (product.CategoryIds != null && product.CategoryIds.Any())
        {
            product.Categories = await _context.Categories
                .Where(c => product.CategoryIds.Contains(c.Id))
                .ToListAsync();
        }

        if (product.Images != null && product.Images.Any())
        {
            // First image becomes the primary ImageUrl
            product.ImageUrl = product.Images.First().Url;
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, Product product)
    {
        if (id != product.Id) return BadRequest();

        var existingProduct = await _context.Products
            .Include(p => p.Categories)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingProduct == null) return NotFound();

        // Update basic fields
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.ImageUrl = product.ImageUrl;
        existingProduct.IsActive = product.IsActive;
        existingProduct.StockQuantity = product.StockQuantity;
        existingProduct.SKU = product.SKU;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        // Sync Categories
        existingProduct.Categories.Clear();
        if (product.CategoryIds != null && product.CategoryIds.Any())
        {
            var categories = await _context.Categories
                .Where(c => product.CategoryIds.Contains(c.Id))
                .ToListAsync();
            foreach (var cat in categories)
            {
                existingProduct.Categories.Add(cat);
            }
        }

        // Sync Images
        _context.ProductImages.RemoveRange(existingProduct.Images);
        existingProduct.Images.Clear();
        if (product.Images != null && product.Images.Any())
        {
            foreach (var img in product.Images)
            {
                existingProduct.Images.Add(new ProductImage { Url = img.Url, ProductId = id });
            }
            existingProduct.ImageUrl = product.Images.First().Url;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Products.Any(e => e.Id == id)) return NotFound();
            else throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id, [FromQuery] bool hardDelete = false)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (hardDelete)
        {
            _context.Products.Remove(product);
        }
        else
        {
            // Soft delete
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int newStock)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.StockQuantity = newStock;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
