using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcommerceApp.Models;
using EcommerceApp.Services;

namespace EcommerceApp.ViewModels;

public class CategoryGroup : ObservableCollection<Product>
{
    public string Name { get; set; }
    public int CategoryId { get; set; }
    public CategoryGroup(string name, int categoryId, IEnumerable<Product> products) : base(products)
    {
        Name = name;
        CategoryId = categoryId;
    }
}

public partial class ProductsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly ICartService _cartService;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AuthButtonText))]
    private bool _isLoggedIn;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategory;

    partial void OnSearchTextChanged(string value) => FilterProducts();
    partial void OnSelectedCategoryChanged(Category? value) => FilterProducts();

    [ObservableProperty]
    private int _cartItemCount;

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<CategoryGroup> GroupedProducts { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    private List<Product> _allProducts = new();

    public string AuthButtonText => IsLoggedIn ? "Logout" : "Registro";

    public ProductsViewModel(IApiService apiService, ICartService cartService)
    {
        _apiService = apiService;
        _cartService = cartService;
        Title = "Shop Products";
        CheckLoginStatus();
        
        _cartService.CartChanged += UpdateCartCount;
        UpdateCartCount();
    }

    private void UpdateCartCount()
    {
        CartItemCount = _cartService.GetCartItems().Sum(i => i.Quantity);
    }

    public async void CheckLoginStatus()
    {
        var token = await Microsoft.Maui.Storage.SecureStorage.Default.GetAsync("jwt_token");
        IsLoggedIn = !string.IsNullOrEmpty(token);
    }

    [RelayCommand]
    public async Task HandleAuthAsync()
    {
        if (IsLoggedIn)
        {
            // Logout
            Microsoft.Maui.Storage.SecureStorage.Default.Remove("jwt_token");
            IsLoggedIn = false;
            OnPropertyChanged(nameof(AuthButtonText));
        }
        else
        {
            // Login
            if (Application.Current?.Windows.Count > 0)
            {
                await Application.Current.Windows[0].Page!.Navigation.PushModalAsync(new Views.LoginPage());
            }
        }
    }

    [RelayCommand]
    private async Task GoToCart()
    {
        if (IsBusy) return;
        IsBusy = true;
        try { await Shell.Current.GoToAsync("//CartPage"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task AddToCartAsync(Product product)
    {
        _cartService.AddToCart(product);
        if (Application.Current?.Windows.Count > 0)
        {
            await Application.Current.Windows[0].Page!.DisplayAlertAsync("Added", $"{product.Name} added to cart.", "OK");
        }
    }

    [RelayCommand]
    public async Task GoToDetails(Product product)
    {
        if (product == null) return;
        await Shell.Current.GoToAsync(nameof(Views.ProductDetailsPage), true, new Dictionary<string, object>
        {
            { "Product", product }
        });
    }

    [RelayCommand]
    public async Task SeeAll(CategoryGroup group)
    {
        if (group == null) return;
        await Shell.Current.GoToAsync("CategoryProductsPage", new Dictionary<string, object>
        {
            { "categoryId", group.CategoryId },
            { "categoryName", group.Name }
        });
    }

    [RelayCommand]
    public async Task SeeAllByCategoryId(Category category)
    {
        if (category == null) return;
        await Shell.Current.GoToAsync("CategoryProductsPage", new Dictionary<string, object>
        {
            { "categoryId", category.Id },
            { "categoryName", category.Name }
        });
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            
            // Load Categories
            var categories = await _apiService.GetCategoriesAsync();
            Categories.Clear();
            Categories.Add(new Category { Id = 0, Name = "All" });
            foreach (var cat in categories) Categories.Add(cat);

            // Load Products
            _allProducts = await _apiService.GetProductsAsync();
            FilterProducts();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading data: {ex.Message}";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FilterProducts()
    {
        var filtered = _allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedCategory != null && SelectedCategory.Id != 0)
        {
             filtered = filtered.Where(p => p.CategoryIds != null && p.CategoryIds.Contains(SelectedCategory.Id));
        }

        // Update GroupedProducts
        var grouped = Categories
            .Where(cat => cat.Id != 0) 
            .Select(cat => 
            {
                var productsInCat = filtered.Where(p => p.CategoryIds != null && p.CategoryIds.Contains(cat.Id))
                                          .Take(6)
                                          .ToList();
                return new CategoryGroup(cat.Name, cat.Id, productsInCat);
            })
            .Where(g => g.Any())
            .ToList();

        GroupedProducts.Clear();
        foreach (var group in grouped)
        {
            GroupedProducts.Add(group);
        }

        // Search/Direct results (optional for top-level scroll)
        Products.Clear();
        foreach (var product in filtered.Take(10))
        {
            Products.Add(product);
        }
    }
}
