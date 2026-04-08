using System;
using System.Collections.Generic;

namespace EcommerceAPI.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Processing";

    public List<OrderItem> Items { get; set; } = new();
}
