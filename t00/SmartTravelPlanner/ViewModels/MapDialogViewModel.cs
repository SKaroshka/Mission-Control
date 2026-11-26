using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using SmartTravelPlanner.Models;
using SmartTravelPlanner.Services;
using System.Linq;

namespace SmartTravelPlanner.ViewModels;

public partial class MapDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string EditorMapFilePath { get; set; } = String.Empty;

    [ObservableProperty]
    public partial ObservableCollection<string> EditorMapFileLines { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<string> EditorFormattedMapFileLines { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveCityCommand))]
    public partial string EditorMapSelection { get; set; } = String.Empty;

    [ObservableProperty]
    public partial string EditorTitle { get; set; } = String.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please enter start point")]
    public partial string EditorStartPoint { get; set; } = String.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please enter end point")]
    public partial string EditorEndPoint { get; set; } = String.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please enter distance between points")]
    public partial string EditorPointsDistance { get; set; } = String.Empty;

    private bool _hasUnsavedChanges = false;

    public Action<bool>? RequestClose { get; set; }

    [GeneratedRegex(@"^(\p{Lu}[\p{L}\p{N}\p{M} '.]*)-(\p{Lu}[\p{L}\p{N}\p{M} '.]*),([0-9]+)$")]
    private static partial Regex MapLinePattern();

    partial void OnEditorStartPointChanged(string value)
    {
        if (value is not null && value != value.Trim())
            EditorStartPoint = value.Trim();
    }

    partial void OnEditorEndPointChanged(string value)
    {
        if (value is not null && value != value.Trim())
            EditorEndPoint = value.Trim();
    }

    public MapDialogViewModel(string mapFilePath, ObservableCollection<string> mapFileLines)
    {
        EditorMapFilePath = mapFilePath;
        EditorMapFileLines = mapFileLines;
        EditorMapFileLines.CollectionChanged += (_, _) => UpdateFormattedMapFileLines();
        UpdateFormattedMapFileLines();
        EditorTitle = $"Editing map file \u2013 {EditorMapFilePath}";
    }

    private void UpdateFormattedMapFileLines()
    {
        EditorFormattedMapFileLines = BaseModel.FormatMapFileLines(EditorMapFileLines);
    }

    private bool IsRemoveButtonEnabled()
    {
        return !String.IsNullOrEmpty(EditorMapSelection);
    }

    private async Task ShowErrorDialog(string errorMessage)
    {
        var errorDialogService = new ErrorDialogService();
        await errorDialogService.ShowErrorDialogAsync(errorMessage);
    }

    [RelayCommand(CanExecute = nameof(IsRemoveButtonEnabled))]
    private async Task RemoveCity()
    {
        if (!String.IsNullOrEmpty(EditorMapSelection) && !String.IsNullOrEmpty(EditorMapFilePath))
        {
            string? mapFileLine = EditorMapFileLines.FirstOrDefault(line =>
                BaseModel.FormatMapFileLines([line]).FirstOrDefault() == EditorMapSelection);

            if (mapFileLine != null)
            {
                EditorMapFileLines.Remove(mapFileLine);
            }

            EditorMapSelection = String.Empty;
            EditorTitle = ($"Editing map file \u2013 {EditorMapFilePath}*");
            _hasUnsavedChanges = true;
        }
        else
        {
            await ShowErrorDialog("No city to remove");
        }
    }

    [RelayCommand]
    private async Task AddCity()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            return;
        }

        if (String.IsNullOrEmpty(EditorMapFilePath))
        {
            await ShowErrorDialog("Please ensure that the file exists");

            return;
        }

        string? newMapFileLine = EditorStartPoint + "-" + EditorEndPoint + "," + EditorPointsDistance;
        if (!MapLinePattern().IsMatch(newMapFileLine))
        {
            newMapFileLine = null;
            await ShowErrorDialog("Use valid format e.g. City Town 404");

            return;
        }

        if (EditorStartPoint == EditorEndPoint)
        {
            newMapFileLine = null;
            await ShowErrorDialog("End city can't have same name as the start city");

            return;
        }

        EditorMapFileLines.Add(newMapFileLine);
        EditorStartPoint = EditorEndPoint = EditorPointsDistance = String.Empty;
        EditorTitle = ($"Editing map file \u2013 {EditorMapFilePath}*");
        _hasUnsavedChanges = true;

        ClearErrors();
    }

    [RelayCommand]
    private async Task SaveMap()
    {
        File.WriteAllLines(EditorMapFilePath, EditorMapFileLines);
        RequestClose?.Invoke(true);
    }

    [RelayCommand]
    private async Task ExitMap()
    {
        RequestClose?.Invoke(false);
    }

    public async Task<bool> CanCloseAsync()
    {
        if (!_hasUnsavedChanges)
            return true;

        var confirmationDialogService = new ConfirmationDialogService();
        return await confirmationDialogService.ShowConfirmationDialogAsync();
    }
}