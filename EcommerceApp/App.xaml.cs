using EcommerceApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
