namespace EcommerceApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        
        Routing.RegisterRoute(nameof(Views.ProductDetailsPage), typeof(Views.ProductDetailsPage));
        Routing.RegisterRoute(nameof(Views.OrderDetailsPage), typeof(Views.OrderDetailsPage));
        Routing.RegisterRoute("CategoryProductsPage", typeof(Views.CategoryProductsPage));
	}
}
