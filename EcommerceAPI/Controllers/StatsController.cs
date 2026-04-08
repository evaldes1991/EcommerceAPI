using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;

namespace EcommerceAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class StatsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StatsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
        var totalOrders = await _context.Orders.CountAsync();
        var totalProducts = await _context.Products.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        return Ok(new
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalProducts = totalProducts,
            TotalUsers = totalUsers
        });
    }
}
