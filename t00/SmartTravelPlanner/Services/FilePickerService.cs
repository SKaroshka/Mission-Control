using System.IO;
using System.Threading.Tasks;
using System.Linq;
//
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace SmartTravelPlanner.Services;

public class FilePickerService : IFilePickerService
{
    private static FilePickerFileType OptionToFilePickerFileType(FileTypeOption option) => option switch
    {
        FileTypeOption.TextPlain => FilePickerFileTypes.TextPlain,
        FileTypeOption.Json => FilePickerFileTypes.Json,
        _ => FilePickerFileTypes.All
    };

    public async Task<string?> OpenFilePickerAsync(string title, string defaultName, params FileTypeOption[] options) {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel is null) return null;

            var fileTypeFilters = options.Select(OptionToFilePickerFileType).ToList();

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                SuggestedFileName=defaultName,
                FileTypeFilter = fileTypeFilters.Count > 0 ? fileTypeFilters : null
            });

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }

        return null;
    }

    public async Task<string?> SaveFilePickerAsync(string title, string defaultName, params FileTypeOption[] options)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel is null) return null;

            var fileTypeFilters = options.Select(OptionToFilePickerFileType).ToList();

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName=defaultName,
                FileTypeChoices = fileTypeFilters.Count > 0 ? fileTypeFilters : null
            });

            return file?.Path.LocalPath;
        }

        return null;
    }
}