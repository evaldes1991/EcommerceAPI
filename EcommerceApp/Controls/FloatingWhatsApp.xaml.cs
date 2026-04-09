using EcommerceApp.Services;

namespace EcommerceApp.Controls;

public partial class FloatingWhatsApp : ContentView
{
    public FloatingWhatsApp()
    {
        InitializeComponent();
    }

    private async void OnWhatsAppClicked(object? sender, EventArgs e)
    {
        var whatsApp = Handler?.MauiContext?.Services.GetService<IWhatsAppService>();
        if (whatsApp != null)
            await whatsApp.OpenChatAsync("Hola, tengo una consulta sobre sus productos.");
    }
}
