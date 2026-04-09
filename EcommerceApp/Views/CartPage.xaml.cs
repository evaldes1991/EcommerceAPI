using Microsoft.Maui.Controls;

namespace EcommerceApp.Views;

public partial class CartPage : ContentPage
{
    private ViewModels.CartViewModel _viewModel;

    public CartPage(ViewModels.CartViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCartItemsCommand.Execute(null);
    }
}
