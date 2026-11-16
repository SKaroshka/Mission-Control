using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
//
using Avalonia.Platform.Storage;

namespace SmartTravelPlanner.Services;

public interface IFilePickerService
{
    Task<string?> OpenFilePickerAsync(string title, string defaultName, params FileTypeOption[] options);
    Task<string?> SaveFilePickerAsync(string title, string defaultName, params FileTypeOption[] options);
}

public interface IMapEditorService
{
    Task<bool> ShowMapDialogAsync(string mapFilePath, ObservableCollection<string> mapFileLines);
    // Task<bool> ShowConfirmationDialog(string title, string message);
}

public interface IErrorDialogService
{
    Task<bool> ShowErrorDialogAsync(string errorMessage);
}

public interface IConfirmationDialogService
{
    Task<bool> ShowConfirmationDialogAsync();
}

public interface IThemeService
{
    event Action? ThemeChanged;
    bool IsDarkTheme { get; }
    void SetTheme(bool IsDark);
    void ToggleTheme();
}