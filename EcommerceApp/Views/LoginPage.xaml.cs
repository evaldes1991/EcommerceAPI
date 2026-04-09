using Microsoft.Maui.Controls;
using EcommerceApp.ViewModels;

namespace EcommerceApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = new AuthViewModel();
    }

    private async void OnSignUpClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new RegisterPage());
    }
}
