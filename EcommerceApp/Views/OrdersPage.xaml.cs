using Microsoft.Maui.Controls;

namespace EcommerceApp.Views;

public partial class OrdersPage : ContentPage
{
    private ViewModels.OrdersViewModel _viewModel;

    public OrdersPage(ViewModels.OrdersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadOrdersCommand.Execute(null);
    }
}
