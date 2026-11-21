using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
//
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
//
using SmartTravelPlanner.Views;
using SmartTravelPlanner.ViewModels;

namespace SmartTravelPlanner.Services;

public class MapEdtorService : IMapEditorService
{
    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        ;
        return null;
    }

    public async Task<bool> ShowMapDialogAsync(string? mapFilePath, ObservableCollection<string> mapFileLines)
    {
        if (mapFilePath == null)
        {
            return false;
        }

        var mainWindow = GetMainWindow();
        if (mainWindow is null) return default;

        var vm = new MapDialogViewModel(mapFilePath, mapFileLines);

        var dialog = new MapDialogWindow(vm);

        return await dialog.ShowDialog<bool>(mainWindow);
    }
}