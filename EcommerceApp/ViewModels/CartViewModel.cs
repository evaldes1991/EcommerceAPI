using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using EcommerceApp.Models;
using EcommerceApp.Services;

namespace EcommerceApp.ViewModels;

public partial class CartViewModel : BaseViewModel
{
    private readonly ICartService _cartService;
    private readonly IApiService _apiService;
    private readonly IWhatsAppService _whatsAppService;

    public ObservableCollection<CartItem> CartItems { get; } = new();

    public decimal TotalAmount => _cartService.GetTotal();

    public CartViewModel(ICartService cartService, IApiService apiService, IWhatsAppService whatsAppService)
    {
        _cartService = cartService;
        _apiService = apiService;
        _whatsAppService = whatsAppService;
        Title = "Shopping Cart";
    }

    [RelayCommand]
    public async Task CheckoutAsync()
    {
        var items = _cartService.GetCartItems();
        if (items.Count == 0) return;

        IsBusy = true;
        try
        {
            await _whatsAppService.SendCheckoutAsync(items, TotalAmount);
            _cartService.ClearCart();
            LoadCartItems();
        }
        catch (Exception ex)
        {
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void LoadCartItems()
    {
        CartItems.Clear();
        foreach (var item in _cartService.GetCartItems())
        {
            CartItems.Add(item);
        }
        OnPropertyChanged(nameof(TotalAmount));
    }

    [RelayCommand]
    public void RemoveFromCart(Product product)
    {
        _cartService.RemoveFromCart(product);
        LoadCartItems();
    }
}
