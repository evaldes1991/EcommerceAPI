using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EcommerceApp.Models;
using EcommerceApp.Services;
using Microsoft.Maui.Storage;

namespace EcommerceApp.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private UserProfile _userProfile = null!;

    [ObservableProperty]
    private bool _isBusy;

    public ProfileViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        IsBusy = true;
        try
        {
            UserProfile = await _apiService.GetProfileAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", "Could not load profile: " + ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void Logout()
    {
        SecureStorage.Default.Remove("jwt_token");
        Shell.Current.GoToAsync("//LoginPage");
    }
}
