using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcommerceApp.Models;
using EcommerceApp.Services;
using System.Collections.ObjectModel;

namespace EcommerceApp.ViewModels;

[QueryProperty(nameof(CategoryId), "categoryId")]
[QueryProperty(nameof(CategoryName), "categoryName")]
public partial class CategoryProductsViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ICartService _cartService;

    [ObservableProperty]
    private string _categoryName = string.Empty;

    [ObservableProperty]
    private int _categoryId;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private int _cartItemCount;

    public ObservableCollection<Product> Products { get; } = new();

    public CategoryProductsViewModel(IApiService apiService, ICartService cartService)
    {
        _apiService = apiService;
        _cartService = cartService;
        UpdateCartCount();
        _cartService.CartChanged += UpdateCartCount;
    }

    private void UpdateCartCount()
    {
        CartItemCount = _cartService.GetCartItems().Sum(i => i.Quantity);
    }

    partial void OnCategoryIdChanged(int value)
    {
        if (value > 0) _ = LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var all = await _apiService.GetProductsAsync();
            Products.Clear();
            foreach (var p in all.Where(p => p.CategoryIds != null && p.CategoryIds.Contains(CategoryId)))
                Products.Add(p);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task GoToDetails(Product product)
    {
        await Shell.Current.GoToAsync("ProductDetailsPage", new Dictionary<string, object>
        {
            ["Product"] = product
        });
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
    private void AddToCart(Product product)
    {
        _cartService.AddToCart(product);
    }
}
