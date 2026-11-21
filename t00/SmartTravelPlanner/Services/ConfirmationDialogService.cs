using System.Threading.Tasks;
using System.Linq;
//
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
//
using SmartTravelPlanner.Views;
using SmartTravelPlanner.ViewModels;

namespace SmartTravelPlanner.Services;

public class ConfirmationDialogService : IConfirmationDialogService
{
    private Window? GetActiveWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.Windows.FirstOrDefault(w => w.IsActive) ?? desktop.MainWindow;
        }
        ;
        return null;
    }

    public async Task<bool> ShowConfirmationDialogAsync()
    {
        var activeWindow = GetActiveWindow();
        if (activeWindow is null) return false;

        var vm = new ConfirmationDialogViewModel();

        var dialog = new ConfirmationDialogWindow(vm);

        return await dialog.ShowDialog<bool>(activeWindow);
    }
}