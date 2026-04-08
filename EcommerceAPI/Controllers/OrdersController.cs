using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EcommerceAPI.Data;
using EcommerceAPI.Models;

namespace EcommerceAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] bool admin = false)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        
        if (role == "Admin" || admin)
        {
            return await _context.Orders.Include(o => o.Items).ThenInclude(i => i.Product).ToListAsync();
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null || !int.TryParse(userIdString, out int userId)) return Unauthorized();

        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .ToListAsync();
    }

    [HttpPatch("{id}/complete")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkComplete(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = "Completed";
        await _context.SaveChangesAsync();
        return NoContent();
    }

    public record PlaceOrderRequest(List<CartItemRequest> Items);
    public record CartItemRequest(int ProductId, int Quantity);

    [HttpPost]
    public async Task<ActionResult<Order>> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

        if (request.Items == null || !request.Items.Any()) return BadRequest("No items in the order.");

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = "Processing"
        };
        
        decimal totalAmount = 0;

        foreach (var reqItem in request.Items)
        {
            var product = await _context.Products.FindAsync(reqItem.ProductId);
            
            if (product == null || !product.IsActive)
                return BadRequest($"Product {reqItem.ProductId} is unavailable.");

            if (product.StockQuantity < reqItem.Quantity)
                return BadRequest($"Insufficient stock for product '{product.Name}'. Requested: {reqItem.Quantity}, Available: {product.StockQuantity}.");

            var orderItem = new OrderItem
            {
                ProductId = reqItem.ProductId,
                Quantity = reqItem.Quantity,
                UnitPrice = product.Price
            };
            
            order.Items.Add(orderItem);
            totalAmount += orderItem.UnitPrice * orderItem.Quantity;

            // Deduct inventory
            product.StockQuantity -= reqItem.Quantity;
        }

        order.TotalAmount = totalAmount;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(order);
    }
}
