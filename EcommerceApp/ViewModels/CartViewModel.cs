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
            // Place the order in the API first
            var order = await _apiService.PlaceOrderAsync(items);

            // Build a friendly message including the order id and summary
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"🛒 Pedido #{order.Id} creado");
            sb.AppendLine("─────────────────");
            foreach (var it in items)
            {
                sb.AppendLine($"▪ {it.Product.Name} x{it.Quantity} — {it.TotalPrice:C}");
            }
            sb.AppendLine("─────────────────");
            sb.AppendLine($"💰 Total: {TotalAmount:C}");
            sb.AppendLine();
            sb.AppendLine($"Hola, acabo de crear el pedido #{order.Id}. Quisiera confirmar los detalles y la forma de pago.");

            // Open WhatsApp with the message
            await _whatsAppService.OpenChatAsync(sb.ToString());

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
