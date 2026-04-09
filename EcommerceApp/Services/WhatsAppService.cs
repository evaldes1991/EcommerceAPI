using System.Text;

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
        var phone = PhoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
        // Try the native WhatsApp URI first (opens the app if installed), then fall back to web URL
        var encoded = string.IsNullOrEmpty(message) ? string.Empty : $"&text={Uri.EscapeDataString(message)}";
        var appUrl = new Uri($"whatsapp://send?phone={phone}{encoded}");
        var webApiUrl = new Uri(string.IsNullOrEmpty(message)
            ? $"https://api.whatsapp.com/send?phone={phone}"
            : $"https://api.whatsapp.com/send?phone={phone}&text={Uri.EscapeDataString(message)}");

        try
        {
            await Launcher.OpenAsync(appUrl);
        }
        catch
        {
            // fallback to web URL if native scheme is not available
            await Launcher.OpenAsync(webApiUrl);
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
