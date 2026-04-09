using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using EcommerceApp.Models;
using EcommerceApp.Services;
using EcommerceApp.Views;

namespace EcommerceApp.ViewModels;

public partial class OrdersViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    public ObservableCollection<Order> Orders { get; } = new();

    public OrdersViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Title = "My Orders";
    }

    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Orders.Clear();
            var orders = await _apiService.GetOrdersAsync();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task GoToDetailsAsync(Order order)
    {
        if (order == null) return;
        await Shell.Current.GoToAsync(nameof(OrderDetailsPage), true, new Dictionary<string, object>
        {
            { "Order", order }
        });
    }
}
