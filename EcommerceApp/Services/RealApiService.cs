using System.Net.Http.Headers;
using System.Net.Http.Json;
using EcommerceApp.Models;
using Microsoft.Maui.Storage;

namespace EcommerceApp.Services;

public class RealApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public RealApiService()
    {
        // Prefer environment variable, then Preferences, otherwise use Railway internal domain
        var env = Environment.GetEnvironmentVariable("API_BASE_URL")
                  ?? Environment.GetEnvironmentVariable("BASE_URL")
                  ?? Environment.GetEnvironmentVariable("BaseUrl");
        var pref = string.Empty;
        try { pref = Preferences.Default.Get("ApiBaseUrl", string.Empty); } catch { }

        if (!string.IsNullOrWhiteSpace(env))
            _baseUrl = env.TrimEnd('/');
        else if (!string.IsNullOrWhiteSpace(pref))
            _baseUrl = pref.TrimEnd('/');
        else
            _baseUrl = "https://ecommerceapi-production-68f0.up.railway.app";

        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    private async Task AddAuthTokenAsync()
    {
        var token = await SecureStorage.Default.GetAsync("jwt_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await AddAuthTokenAsync();
        var response = await _httpClient.GetAsync(new Uri(new Uri(_baseUrl), "api/products"));
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Product>>() ?? new List<Product>();
        }
        return new List<Product>();
    }

    public async Task<Order> PlaceOrderAsync(List<CartItem> cartItems)
    {
        await AddAuthTokenAsync();

        var requestBody = new
        {
            Items = cartItems.Select(c => new { ProductId = c.Product.Id, Quantity = c.Quantity }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync(new Uri(new Uri(_baseUrl), "api/orders"), requestBody);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error: {errorContent}");
        }

        var order = await response.Content.ReadFromJsonAsync<Order>();
        return order ?? throw new Exception("Error placing order parsing response.");
    }

    public async Task<List<Order>> GetOrdersAsync()
    {
        await AddAuthTokenAsync();
        var response = await _httpClient.GetAsync(new Uri(new Uri(_baseUrl), "api/orders"));
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Order>>() ?? new List<Order>();
        }
        return new List<Order>();
    }

    public async Task<UserProfile> GetProfileAsync()
    {
        await AddAuthTokenAsync();
        var response = await _httpClient.GetAsync(new Uri(new Uri(_baseUrl), "api/users/profile"));
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserProfile>() ?? new UserProfile();
        }
        throw new Exception("Error fetching profile.");
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync(new Uri(new Uri(_baseUrl), "api/categories"));
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Category>>() ?? new List<Category>();
        }
        return new List<Category>();
    }
}
