using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EcommerceApp.Models;
using System;

namespace EcommerceApp.Services;

public interface IApiService
{
    Task<List<Product>> GetProductsAsync();
    Task<Order> PlaceOrderAsync(List<CartItem> cartItems);
    Task<List<Order>> GetOrdersAsync();
    Task<UserProfile> GetProfileAsync();
    Task<List<Category>> GetCategoriesAsync();
}

public class MockApiService : IApiService
{
    private List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Premium Wireless Headphones", Description = "High-quality noise-canceling headphones.", Price = 250.00m, ImageUrl = "https://picsum.photos/400/400?random=1", CategoryIds = new List<int> { 2 } },
        new Product { Id = 2, Name = "Smartphone Case", Description = "Durable silicone smartphone case.", Price = 25.00m, ImageUrl = "https://picsum.photos/400/400?random=2", CategoryIds = new List<int> { 2 } },
        new Product { Id = 3, Name = "Smartwatch", Description = "Fitness and health tracker smartwatch.", Price = 150.00m, ImageUrl = "https://picsum.photos/400/400?random=3", CategoryIds = new List<int> { 2 } },
        new Product { Id = 4, Name = "Laptop Stand", Description = "Adjustable aluminum laptop stand.", Price = 45.00m, ImageUrl = "https://picsum.photos/400/400?random=4", CategoryIds = new List<int> { 2 } },
        new Product { Id = 5, Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard with tactile switches.", Price = 120.00m, ImageUrl = "https://picsum.photos/400/400?random=5", CategoryIds = new List<int> { 2 } }
    };

    private List<Order> _orders = new List<Order>();

    public async Task<List<Product>> GetProductsAsync()
    {
        await Task.Delay(500); // Simulate network delay
        return _products;
    }

    public async Task<Order> PlaceOrderAsync(List<CartItem> cartItems)
    {
        await Task.Delay(1000);
        var order = new Order
        {
            Id = new Random().Next(1000, 9999),
            OrderDate = DateTime.Now,
            Items = cartItems.Select(c => new OrderItem
            {
                ProductId = c.Product.Id,
                Product = c.Product,
                Quantity = c.Quantity,
                UnitPrice = c.Product.Price
            }).ToList(),
            TotalAmount = cartItems.Sum(c => c.TotalPrice),
            Status = "Processing"
        };
        _orders.Add(order);
        return order;
    }

    public async Task<List<Order>> GetOrdersAsync()
    {
        await Task.Delay(500);
        return _orders;
    }

    public async Task<UserProfile> GetProfileAsync()
    {
        await Task.Delay(500);
        return new UserProfile { Id = 1, Email = "test@example.com", Role = "User" };
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        await Task.Delay(500);
        return new List<Category>
        {
            new Category { Id = 1, Name = "Shoes" },
            new Category { Id = 2, Name = "Electronics" },
            new Category { Id = 3, Name = "Clothing" }
        };
    }
}
