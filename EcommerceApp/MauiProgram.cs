using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using EcommerceApp.Services;
using EcommerceApp.ViewModels;
using EcommerceApp.Views;

namespace EcommerceApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<IApiService, RealApiService>();
		builder.Services.AddSingleton<ICartService, CartService>();
		
		// Views
		builder.Services.AddTransient<ProductsPage>();
		builder.Services.AddTransient<ProductDetailsPage>();
		builder.Services.AddTransient<CartPage>();
		builder.Services.AddTransient<OrdersPage>();
		builder.Services.AddTransient<ProfilePage>();
		builder.Services.AddTransient<OrderDetailsPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<CategoryProductsPage>();

		// ViewModels
		builder.Services.AddTransient<ProductsViewModel>();
		builder.Services.AddTransient<ProductDetailsViewModel>();
		builder.Services.AddTransient<CartViewModel>();
		builder.Services.AddTransient<OrdersViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<OrderDetailsViewModel>();
		builder.Services.AddTransient<CategoryProductsViewModel>();

		return builder.Build();
	}
}
