using Microsoft.Maui.Controls;
using EcommerceApp.Models;
using EcommerceApp.ViewModels;

namespace EcommerceApp.Views;

public partial class ProductDetailsPage : ContentPage
{
	public ProductDetailsPage(ProductDetailsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
