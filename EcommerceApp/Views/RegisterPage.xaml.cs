using Microsoft.Maui.Controls;
using EcommerceApp.ViewModels;

namespace EcommerceApp.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
        BindingContext = new AuthViewModel();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
