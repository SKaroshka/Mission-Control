using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using SmartTravelPlanner.Services;

namespace SmartTravelPlanner.ViewModels;

public partial class ErrorDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = String.Empty;

    public Action<bool>? RequestClose { get; set; }

    public ErrorDialogViewModel(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    [RelayCommand]
    private async Task Exit()
    {
        RequestClose?.Invoke(false);
    }
}