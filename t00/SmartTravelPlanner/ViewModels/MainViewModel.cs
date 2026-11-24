using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using SmartTravelPlanner.Models;
using SmartTravelPlanner.Services;
using Avalonia.Controls.ApplicationLifetimes;


namespace SmartTravelPlanner.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // View manipulations
    [ObservableProperty]
    public partial int CurrentPageIndex { get; set; } = 0;

    [ObservableProperty]
    public partial bool IsNameView { get; set; } = true;

    [ObservableProperty]
    public partial bool IsMapView { get; set; } = false;

    [ObservableProperty]
    public partial bool IsInputView { get; set; } = false;

    [ObservableProperty]
    public partial bool IsOutputView { get; set; } = false;

    private readonly IThemeService _themeService = new ThemeService();

    [ObservableProperty]
    public partial bool IsDarkTheme { get; set; }

    [ObservableProperty]
    public partial string ThemeIcon { get; set; } = String.Empty;

    [RelayCommand]
    private async Task GoToNameView()
    {
        CurrentPageIndex = 0;
        IsNameView = true;
        IsMapView = false;
        IsInputView = false;
        IsOutputView = false;
        FormattedSavedData = BaseModel.FormatSavedData(TravelerFile, MapFileName,
                                                    StartLocation, Destination,
                                                    Route, Distance);
        ClearErrors();
    }

    [RelayCommand]
    private async Task GoToMapView()
    {
        ValidateProperty(TravelerFile, nameof(TravelerFile));

        if (GetErrors(nameof(TravelerFile)).Any())
        {
            return;
        }

        CurrentPageIndex = 1;
        IsNameView = false;
        IsMapView = true;
        IsInputView = false;
        IsOutputView = false;
        FormattedSavedData = BaseModel.FormatSavedData(TravelerFile, MapFileName,
                                                    StartLocation, Destination,
                                                    Route, Distance);
        ClearErrors();
    }

    [RelayCommand]
    private async Task GoToInputView()
    {
        ValidateProperty(MapFileName, nameof(MapFileName));

        if (GetErrors(nameof(MapFileName)).Any())
        {
            return;
        }

        CurrentPageIndex = 2;
        IsNameView = false;
        IsMapView = false;
        IsInputView = true;
        IsOutputView = false;
        FormattedSavedData = BaseModel.FormatSavedData(TravelerFile, MapFileName,
                                                    StartLocation, Destination,
                                                    Route, Distance);
        ClearErrors();
    }

    [RelayCommand]
    private async Task GoToOutputView()
    {
        CurrentPageIndex = 3;
        IsNameView = false;
        IsMapView = false;
        IsInputView = false;
        IsOutputView = true;
        FormattedSavedData = BaseModel.FormatSavedData(TravelerFile, MapFileName,
                                                    StartLocation, Destination,
                                                    Route, Distance);
        ClearErrors();
    }

    private void UpdateThemeIcon()
    {
        ThemeIcon = _themeService.IsDarkTheme ? "\u2600" : "\u263E";
    }

    [RelayCommand]
    private async Task ToggleTheme()
    {
        _themeService.ToggleTheme();
        UpdateThemeIcon();
    }

    // All-app interactions
    public Action? ExitAction { get; set; }

    [RelayCommand]
    private void Exit() => ExitAction?.Invoke();

    // Main
    // Input properties
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please enter traveler's name")]
    public partial string? TravelerFile { get; set; } = null;

    [ObservableProperty]
    public partial string LoadMapButtonContent { get; set; } = "Load map file...";

    [ObservableProperty]
    public partial string? MapFilePath { get; set; } = String.Empty;

    [ObservableProperty]
    public partial bool IsOpenEditorButtonEnabled { get; set; } = false;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please enter start location")]
    [IsCity(nameof(CitiesCollection))]
    public partial string StartLocation { get; set; } = String.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Please enter destination")]
    [IsCity(nameof(CitiesCollection))]
    public partial string Destination { get; set; } = String.Empty;

    // Output properties
    [ObservableProperty]
    public partial string Route { get; set; } = String.Empty;

    [ObservableProperty]
    public partial string Distance { get; set; } = String.Empty;

    [ObservableProperty]
    public partial string FormattedSavedData { get; set; } = String.Empty;

    [ObservableProperty]
    public partial bool IsSaveTravelerMenuItemEnabled { get; set; } = false;

    [ObservableProperty]
    public partial ObservableCollection<string> MapFileLines { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<string> FormattedMapFileLinesCollection { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<string> CitiesCollection { get; set; } = [];

    [ObservableProperty]
    [Required(ErrorMessage = "Please load map file")]
    public partial string MapFileName { get; set; } = String.Empty;

    [ObservableProperty]
    public partial string MapPreview { get; set; } = String.Empty;

    // Editor-related
    [ObservableProperty]
    public partial bool IsEditorSavedMap { get; set; } = false;

    private Traveler? _traveler;

    [GeneratedRegex(@"^([\p{L}\p{N}\p{M} '.]+)-([\p{L}\p{N}\p{M} '.]+),([0-9]+)$")]
    private static partial Regex MapFileLinePattern();

    public MainViewModel()
    {
        _themeService.ThemeChanged += UpdateThemeIcon;
        UpdateThemeIcon();
    }

    partial void OnTravelerFileChanged(string? value)
    {
        if (value is not null && value != value.Trim())
            TravelerFile = value.Trim();
    }

    private void MapUIUpdate()
    {
        if (!String.IsNullOrEmpty(MapFilePath))
        {
            IsOpenEditorButtonEnabled = true;
            LoadMapButtonContent = "Change file...";
            MapFileName = $"{System.IO.Path.GetFileName(MapFilePath)} — map loaded";
            MapFileLines = BaseModel.UpdateMapFileLines(MapFilePath, MapFileLines);
            MapPreview = String.Join(Environment.NewLine, BaseModel.FormatMapFileLines(MapFileLines));
        }
        else
        {
            IsOpenEditorButtonEnabled = false;
            LoadMapButtonContent = "Load map file...";
            MapFileName = String.Empty;
            MapFileLines.Clear();
            MapPreview = String.Empty;
        }
    }

    private async Task OpenMapEditor()
    {
        MapFileLines = BaseModel.UpdateMapFileLines(MapFilePath, MapFileLines);
        var dialogMapFileLines = new ObservableCollection<string>(MapFileLines);

        var mapEdtorService = new MapEdtorService();
        IsEditorSavedMap = await mapEdtorService.ShowMapDialogAsync(MapFilePath, dialogMapFileLines);

        BaseModel.UpdateMapFileLines(MapFilePath, MapFileLines);
        CitiesCollection = BaseModel.GetCitiesCollection(MapFileLines) ?? [];
        MapPreview = String.Join(Environment.NewLine, BaseModel.FormatMapFileLines(BaseModel.UpdateMapFileLines(MapFilePath, MapFileLines)));
    }

    private async Task ShowErrorDialog(string errorMessage)
    {
        var errorDialogService = new ErrorDialogService();
        await errorDialogService.ShowErrorDialogAsync(errorMessage);
    }

    [RelayCommand]
    private async Task ClearRoute()
    {
        if (_traveler is not null)
        {
            _traveler.ClearRoute();
            Route = _traveler.GetRoute();
            Distance = String.Empty;
            IsSaveTravelerMenuItemEnabled = false;
            await GoToInputView();
        }
    }

    [RelayCommand]
    private async Task LoadMap()
    {
        var filePickerService = new FilePickerService();
        MapFilePath = await filePickerService.OpenFilePickerAsync("Load map file", "map.txt", FileTypeOption.TextPlain);

        if (!String.IsNullOrEmpty(MapFilePath))
        {
            foreach (string line in File.ReadLines(MapFilePath))
            {
                if (String.IsNullOrWhiteSpace(line)) continue;

                if (!MapFileLinePattern().IsMatch(line))
                {
                    MapFilePath = String.Empty;
                    MapUIUpdate();

                    await ShowErrorDialog("Invalid map format");

                    return;
                }
            }
            MapFileLines.Clear();
            MapUIUpdate();
            CitiesCollection = BaseModel.GetCitiesCollection(MapFileLines) ?? [];
        }
    }

    [RelayCommand]
    private async Task UnloadMap()
    {
        MapFilePath = String.Empty;
        MapUIUpdate();
        FormattedMapFileLinesCollection.Clear();
        CitiesCollection.Clear();
        IsEditorSavedMap = false;
    }

    [RelayCommand]
    private async Task PlanRoute()
    {
        ValidateProperty(StartLocation, nameof(StartLocation));

        if (GetErrors(nameof(StartLocation)).Any() || GetErrors(nameof(Destination)).Any())
        {
            return;
        }

        var planRouteResults = await BaseModel.PlanRoute(MapFilePath, TravelerFile, StartLocation, Destination);

        if (planRouteResults is null)
            return;


        if (!String.IsNullOrEmpty(planRouteResults[2]))
        {
            await ShowErrorDialog(planRouteResults[2]);
            return;
        }

        Route = planRouteResults[0];
        Distance = planRouteResults[1];
        _traveler = BaseModel.GetTravelerClone(TravelerFile);
        IsSaveTravelerMenuItemEnabled = true;
        await GoToOutputView();
    }

    [RelayCommand]
    private async Task SaveTraveler()
    {
        if (_traveler is null)
        {
            await ShowErrorDialog("Please plan route before saving your traveler's data");
            return;
        }

        var filePickerService = new FilePickerService();
        var filePath = await filePickerService.SaveFilePickerAsync("Save traveler's file", $"{TravelerFile}.json", FileTypeOption.Json);

        if (!String.IsNullOrEmpty(filePath)) _traveler.SaveToFile(filePath);
    }

    [RelayCommand]
    private async Task LoadTraveler()
    {
        var filePickerService = new FilePickerService();
        var filePath = await filePickerService.OpenFilePickerAsync("Load traveler's file", "", FileTypeOption.Json);
        if (!String.IsNullOrEmpty(filePath))
        {
            try
            {
                _traveler = Traveler.LoadFromFile(filePath);
            }
            catch (Exception)
            {
                await ShowErrorDialog("Invalid JSON Format");
                return;
            }
            TravelerFile = _traveler.GetName();
            StartLocation = _traveler.GetLocation();
            Destination = String.Empty;
            Route = _traveler.GetRoute();
            Distance = String.Empty;
            await GoToMapView();
        }
    }

    [RelayCommand]
    private async Task OpenEditor()
    {
        await OpenMapEditor();
    }

    [RelayCommand]
    private async Task CreateNewMap()
    {
        var filePickerService = new FilePickerService();
        var filePath = await filePickerService.SaveFilePickerAsync("Create new map file", "map.txt", FileTypeOption.TextPlain);

        if (!String.IsNullOrEmpty(filePath))
        {
            File.WriteAllLines(filePath, ["City1-City2,404"]);
            MapFileLines.Clear();
            MapFilePath = filePath;
            MapUIUpdate();
            await OpenMapEditor();
        }
    }

    [RelayCommand]
    private async Task CreateNewTraveler()
    {
        _traveler = null;
        TravelerFile = null;
        MapFilePath = String.Empty;
        MapUIUpdate();
        StartLocation = Destination = Route = Distance = String.Empty;
        FormattedSavedData = String.Empty;

        FormattedMapFileLinesCollection.Clear();
        CitiesCollection.Clear();

        IsEditorSavedMap = false;
        IsSaveTravelerMenuItemEnabled = false;

        await GoToNameView();
    }
}
