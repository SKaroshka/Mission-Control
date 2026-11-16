using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using SmartTravelPlanner.Services;

namespace SmartTravelPlanner.ViewModels;

public partial class ConfirmationDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = String.Empty;

    public Action<bool>? RequestClose { get; set; }

    [RelayCommand]
    private void Confirm()
    {
        RequestClose?.Invoke(true);
    }

    [RelayCommand]
    private async Task Cancel()
    {
        RequestClose?.Invoke(false);
    }
}