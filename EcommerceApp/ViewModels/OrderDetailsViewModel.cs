using CommunityToolkit.Mvvm.ComponentModel;
using EcommerceApp.Models;

namespace EcommerceApp.ViewModels;

[QueryProperty(nameof(Order), "Order")]
public partial class OrderDetailsViewModel : BaseViewModel
{
    [ObservableProperty]
    private Order _order = null!;

    public OrderDetailsViewModel()
    {
        Title = "Order Details";
    }
}
