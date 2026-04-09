using System.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Core;

namespace EcommerceApp.Services;

public interface IWhatsAppService
{
    string PhoneNumber { get; set; }
    Task OpenChatAsync(string? message = null);
    Task SendCheckoutAsync(List<Models.CartItem> items, decimal total);
}

public class WhatsAppService : IWhatsAppService
{
    private const string DefaultPhone = "+19792691852"; // Replace with your number

    public string PhoneNumber
    {
        get => Preferences.Get("whatsapp_phone", DefaultPhone);
        set => Preferences.Set("whatsapp_phone", value);
    }

    public async Task OpenChatAsync(string? message = null)
    {
        var phone = PhoneNumber?.Replace("+", "").Replace(" ", "").Replace("-", "") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(phone))
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Número no configurado", "El número de WhatsApp no está configurado. Por favor configura el número en la app.", "OK");
            return;
        }

        var encodedText = string.IsNullOrEmpty(message) ? string.Empty : Uri.EscapeDataString(message);

        var appUri = new Uri(string.IsNullOrEmpty(encodedText)
            ? $"whatsapp://send?phone={phone}"
            : $"whatsapp://send?phone={phone}&text={encodedText}");

        var webUri = new Uri(string.IsNullOrEmpty(encodedText)
            ? $"https://api.whatsapp.com/send?phone={phone}"
            : $"https://api.whatsapp.com/send?phone={phone}&text={encodedText}");

        try
        {
            // prefer native app
            if (await Launcher.CanOpenAsync(appUri))
            {
                await Launcher.OpenAsync(appUri);
                return;
            }

            // fallback to web
            if (await Launcher.CanOpenAsync(webUri))
            {
                await Launcher.OpenAsync(webUri);
                return;
            }

            // neither available - copy web URL to clipboard and inform user
            await Clipboard.SetTextAsync(webUri.ToString());
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("WhatsApp no disponible", "No se pudo abrir WhatsApp ni la versión web en este dispositivo. Se copió la URL al portapapeles.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WhatsApp OpenChatAsync error: {ex}");
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo abrir WhatsApp.", "OK");
            }
        }
    }

    public async Task SendCheckoutAsync(List<Models.CartItem> items, decimal total)
    {
        if (items.Count == 0) return;

        var sb = new StringBuilder();
        sb.AppendLine("🛒 *Nuevo Pedido - Antigravity*");
        sb.AppendLine("─────────────────");

        foreach (var item in items)
        {
            sb.AppendLine($"▪ {item.Product.Name} x{item.Quantity} — {item.TotalPrice:C}");
        }

        sb.AppendLine("─────────────────");
        sb.AppendLine($"💰 *Total: {total:C}*");
        sb.AppendLine();
        sb.AppendLine("Hola, me gustaría realizar este pedido. ¿Podrían confirmarme disponibilidad y forma de pago?");

        await OpenChatAsync(sb.ToString());
    }
}
