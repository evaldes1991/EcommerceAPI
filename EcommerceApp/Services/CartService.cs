using System.Collections.Generic;
using System.Linq;
using EcommerceApp.Models;

namespace EcommerceApp.Services;

public interface ICartService
{
    event Action CartChanged;
    List<CartItem> GetCartItems();
    void AddToCart(Product product);
    void RemoveFromCart(Product product);
    void ClearCart();
    decimal GetTotal();
}

public class CartService : ICartService
{
    public event Action? CartChanged;
    private List<CartItem> _items = new List<CartItem>();

    public List<CartItem> GetCartItems() => _items.ToList();

    public void AddToCart(Product product)
    {
        var existingItem = _items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            _items.Add(new CartItem { Product = product, Quantity = 1 });
        }
        CartChanged?.Invoke();
    }

    public void RemoveFromCart(Product product)
    {
        var existingItem = _items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity--;
            if (existingItem.Quantity <= 0)
            {
                _items.Remove(existingItem);
            }
        }
        CartChanged?.Invoke();
    }

    public void ClearCart()
    {
        _items.Clear();
        CartChanged?.Invoke();
    }

    public decimal GetTotal()
    {
        return _items.Sum(i => i.TotalPrice);
    }
}
