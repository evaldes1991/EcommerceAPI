using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcommerceApp.Models;
using EcommerceApp.Services;

namespace EcommerceApp.ViewModels;

[QueryProperty(nameof(Product), "Product")]
public partial class ProductDetailsViewModel : BaseViewModel
{
    private readonly ICartService _cartService;

    [ObservableProperty]
    private Product _product = null!;

    [ObservableProperty]
    private int _cartItemCount;

    public ProductDetailsViewModel(ICartService cartService)
    {
        _cartService = cartService;
        Title = "Details";
        _cartService.CartChanged += UpdateCartCount;
        UpdateCartCount();
    }

    private void UpdateCartCount()
    {
        CartItemCount = _cartService.GetCartItems().Sum(i => i.Quantity);
    }

    [RelayCommand]
    private async Task GoToCart()
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await Shell.Current.GoToAsync("//CartPage"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task AddToCartAsync()
    {
        if (Product == null) return;
        _cartService.AddToCart(Product);
        await Shell.Current.DisplayAlertAsync("Added", $"{Product.Name} added to cart.", "OK");
    }
}
