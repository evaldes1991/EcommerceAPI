using Microsoft.Maui.Controls;

namespace EcommerceApp.Views;

public partial class ProductsPage : ContentPage
{
    private ViewModels.ProductsViewModel _viewModel;

    public ProductsPage(ViewModels.ProductsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadProductsCommand.Execute(null);
    }
}
