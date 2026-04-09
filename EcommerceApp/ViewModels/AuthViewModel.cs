using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;

namespace EcommerceApp.ViewModels;

public partial class AuthViewModel : BaseViewModel
{
    private readonly HttpClient _httpClient;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public AuthViewModel()
    {
        // For local testing without SSL issues, point to the HTTP profile's port
        _httpClient = new HttpClient { BaseAddress = new Uri("http://192.168.1.84:5125") };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password)) return;
        
        IsBusy = true;
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { Email, Password });
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    await SecureStorage.Default.SetAsync("jwt_token", result.Token);
                    
                    if (Application.Current?.Windows.Count > 0)
                    {
                        Application.Current.Windows[0].Page = new AppShell(); // Navigate to main app
                    }
                }
            }
            else
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("Error", "Invalid Login", "OK");
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password)) return;

        IsBusy = true;
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", new { Email, Password });
            if (response.IsSuccessStatusCode)
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("Success", "Registered! You can now login.", "OK");
                }
            }
            else
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("Error", "Registration failed", "OK");
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private class LoginResult
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
