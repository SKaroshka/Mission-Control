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
using SmartTravelPlanner.Views;


namespace SmartTravelPlanner.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // View-related
    [ObservableProperty]
    public partial int CurrentPageIndex { get; set; } = 0;

    [ObservableProperty]
    public partial bool IsNameViewEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsMapViewEnabled { get; set; } = false;

    [ObservableProperty]
    public partial bool IsInputViewEnabled { get; set; } = false;

    [ObservableProperty]
    public partial bool IsOutputViewEnabled { get; set; } = false;

    [ObservableProperty]
    public partial bool IsOnNameView { get; set; } = true;

    [ObservableProperty]
    public partial bool IsOnMapView { get; set; } = false;

    [ObservableProperty]
    public partial bool IsOnInputView { get; set; } = false;

    [ObservableProperty]
    public partial bool IsOnOutputView { get; set; } = false;

    private readonly IThemeService _themeService = new ThemeService();

    [ObservableProperty]
    public partial bool IsDarkTheme { get; set; }

    [ObservableProperty]
    public partial string ThemeIcon { get; set; } = String.Empty;

    public enum ViewStatus
    {
        Pending,
        Error,
        Correct
    }

    [ObservableProperty]
    public partial ViewStatus NameViewStatus { get; set; } = ViewStatus.Pending;

    [ObservableProperty]
    public partial ViewStatus MapViewStatus { get; set; } = ViewStatus.Pending;

    [ObservableProperty]
    public partial ViewStatus InputViewStatus { get; set; } = ViewStatus.Pending;

    [ObservableProperty]
    public partial ViewStatus OutputViewStatus { get; set; } = ViewStatus.Pending;

    [RelayCommand]
    private async Task GoToNameView()
    {
        ValidateProperty(TravelerFile, nameof(TravelerFile));
        ValidateProperty(StartLocation, nameof(StartLocation));

        if (_traveler is null)
            ClearErrors();

        CurrentPageIndex = 0;
        if (!IsMapViewEnabled)
        {
            IsNameViewEnabled = true;
            IsMapViewEnabled = false;
            IsInputViewEnabled = false;
            IsOutputViewEnabled = false;
        }

        IsOnNameView = true;
        IsOnMapView = false;
        IsOnInputView = false;
        IsOnOutputView = false;

        FormattedSavedData = BaseModel.FormatSavedData(TravelerFile, MapFileName,
                                                    StartLocation, Destination,
                                                    Route, Distance);
    }

    [RelayCommand]
    private async Task GoToMapView()
    {
        ValidateProperty(TravelerFile, nameof(TravelerFile));
        ValidateProperty(StartLocation, nameof(StartLocation));

        if (GetErrors(nameof(TravelerFile)).Any() || (GetErrors(nameof(StartLocation)).Any() && !CitiesCollection.Any())
        || String.IsNullOrWhiteSpace(StartLocation))
        {
            NameViewStatus = ViewStatus.Error;
            return;
        }

        if (GetErrors(nameof(StartLocation)).Any() && CitiesCollection.Any() && IsOnNameView)
        {
            NameViewStatus = ViewStatus.Error;
        }

        if (!GetErrors(nameof(StartLocation)).Any())
        {
            NameViewStatus = ViewStatus.Correct;
        }

        if ((IsOnInputView || IsOnOutputView) && !File.Exists(MapFilePath))
        {
            MapFilePath = String.Empty;

            await ShowErrorDialog("Map file doesn't exist");
        }

        if (!String.IsNullOrEmpty(MapFilePath))
        {
            foreach (string line in File.ReadLines(MapFilePath))
            {
                if (String.IsNullOrWhiteSpace(line)) continue;

                if (!MapFileLinePattern().IsMatch(line))
                {
                    MapFilePath = String.Empty;

                    await ShowErrorDialog("Invalid map format");
                    break;
                }
            }
        }

        CurrentPageIndex = 1;
        if (!IsInputViewEnabled)
        {
            IsNameViewEnabled = true;
            IsMapViewEnabled = true;
            IsInputViewEnabled = false;
            IsOutputViewEnabled = false;
        }

        IsOnNameView = false;
        IsOnMapView = true;
        IsOnInputView = false;
        IsOnOutputView = false;

        MapUIUpdate();

        if (TravelerFile != null)
        {
            _traveler = new(TravelerFile);
            _traveler.SetLocation(StartLocation);
            IsSaveTravelerMenuItemEnabled = true;
        }

        FormattedSavedData = BaseModel.FormatSavedData(TravelerFile, MapFileName,
                                                    StartLocation, Destination,
                                                    Route, Distance);
        ClearErrors();
    }

    [RelayCommand]
    private async Task GoToInputView()
    {
        ValidateProperty(TravelerFile, nameof(TravelerFile));
        ValidateProperty(StartLocation, nameof(StartLocation));
        ValidateProperty(MapFileName, nameof(MapFileName));

        if (GetErrors(nameof(TravelerFile)).Any() || (GetErrors(nameof(StartLocation)).Any() && !CitiesCollection.Any()))
        {
            NameViewStatus = ViewStatus.Error;
            return;
        }

        if (GetErrors(nameof(StartLocation)).Any() && CitiesCollection.Any() && IsOnNameView)
        {
            NameViewStatus = ViewStatus.Error;
        }

        if (!GetErrors(nameof(StartLocation)).Any())
        {
            NameViewStatus = ViewStatus.Correct;
        }

        if (GetErrors(nameof(MapFileName)).Any())
        {
            MapViewStatus = ViewStatus.Error;
            return;
        }

        MapViewStatus = ViewStatus.Correct;
        CurrentPageIndex = 2;
        if (!IsOutputViewEnabled)
        {
            IsNameViewEnabled = true;
            IsMapViewEnabled = true;
            IsInputViewEnabled = true;
            IsOutputViewEnabled = false;
        }

        IsOnNameView = false;
        IsOnMapView = false;
        IsOnInputView = true;
        IsOnOutputView = false;

        FormattedSavedData = BaseModel.FormatSavedData(TravelerFile, MapFileName,
                                                    StartLocation, Destination,
                                                    Route, Distance);
        ClearErrors();
    }

    [RelayCommand]
    private async Task GoToOutputView()
    {
        OutputViewStatus = ViewStatus.Correct;

        CurrentPageIndex = 3;
        IsNameViewEnabled = true;
        IsMapViewEnabled = true;
        IsInputViewEnabled = true;
        IsOutputViewEnabled = true;

        IsOnNameView = false;
        IsOnMapView = false;
        IsOnInputView = false;
        IsOnOutputView = true;

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
    [Required(ErrorMessage = "Please enter current location")]
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

    [GeneratedRegex(@"^(\p{Lu}[\p{L}\p{N}\p{M} '.]*)-(\p{Lu}[\p{L}\p{N}\p{M} '.]*),([0-9]+)$")]
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

    private async static Task ShowErrorDialog(string errorMessage)
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

            IsOutputViewEnabled = false;
            OutputViewStatus = ViewStatus.Pending;

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

            if (!File.Exists(MapFilePath))
            {
                MapFilePath = String.Empty;
                MapUIUpdate();

                await ShowErrorDialog("This file doesn't exist");

                return;
            }

            if (String.IsNullOrWhiteSpace(File.ReadAllText(MapFilePath)))
            {
                MapFilePath = String.Empty;
                MapUIUpdate();

                await ShowErrorDialog("This file is empty");

                return;
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
        if (!File.Exists(MapFilePath))
        {
            await GoToMapView();
            return;
        }

        if (String.IsNullOrWhiteSpace(File.ReadAllText(MapFilePath)))
        {
            await GoToMapView();
            return;
        }

        foreach (string line in File.ReadLines(MapFilePath))
        {
            if (String.IsNullOrWhiteSpace(line)) continue;

            if (!MapFileLinePattern().IsMatch(line))
            {
                await GoToMapView();
                return;
            }
        }

        ValidateProperty(StartLocation, nameof(StartLocation));
        ValidateProperty(Destination, nameof(Destination));

        bool startLocationHasErrors = false;

        if (GetErrors(nameof(StartLocation)).Any())
        {
            NameViewStatus = ViewStatus.Error;
            await ShowErrorDialog($"Please ensure that your current location is on the map.{Environment.NewLine}Check for typo or add it to the map");
            await GoToMapView();
            startLocationHasErrors = true;
        }

        if (GetErrors(nameof(Destination)).Any())
        {
            InputViewStatus = ViewStatus.Error;
            return;
        }

        InputViewStatus = ViewStatus.Correct;

        if (startLocationHasErrors)
        {
            return;
        }

        NameViewStatus = ViewStatus.Correct;

        MapUIUpdate();

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
        var filePath = await filePickerService.SaveFilePickerAsync("Save traveler's file", "traveler.json", FileTypeOption.Json);

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

            IsInputViewEnabled = IsOutputViewEnabled = false;
            NameViewStatus = MapViewStatus =
                InputViewStatus = OutputViewStatus = ViewStatus.Pending;

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

        IsMapViewEnabled = IsInputViewEnabled = IsOutputViewEnabled = false;
        NameViewStatus = MapViewStatus =
            InputViewStatus = OutputViewStatus = ViewStatus.Pending;

        IsOnNameView = true;
        await GoToNameView();
    }
}
