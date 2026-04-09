using EcommerceApp.ViewModels;

namespace EcommerceApp.Views;

public partial class CategoryProductsPage : ContentPage
{
    public CategoryProductsPage(CategoryProductsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
