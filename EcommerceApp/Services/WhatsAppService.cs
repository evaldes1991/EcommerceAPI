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
    private const string DefaultPhone = "5350000000"; // Replace with your number

    public string PhoneNumber
    {
        get => Preferences.Get("whatsapp_phone", DefaultPhone);
        set => Preferences.Set("whatsapp_phone", value);
    }

    public async Task OpenChatAsync(string? message = null)
    {
        var phone = PhoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");
        var url = string.IsNullOrEmpty(message)
            ? $"https://wa.me/{phone}"
            : $"https://wa.me/{phone}?text={Uri.EscapeDataString(message)}";

        await Launcher.OpenAsync(new Uri(url));
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
