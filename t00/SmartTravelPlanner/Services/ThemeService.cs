using System;
using Avalonia;
using Avalonia.Styling;

namespace SmartTravelPlanner.Services;

public class ThemeService : IThemeService
{
    public event Action? ThemeChanged;

    public ThemeService()
    {
        if (Application.Current is not null)
        {
            Application.Current.ActualThemeVariantChanged += (_, _) => ThemeChanged?.Invoke();
        }
    }

    public bool IsDarkTheme => 
        Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

    public void SetTheme(bool IsDark)
    {
        if (Application.Current is not null)
        {
            Application.Current.RequestedThemeVariant = IsDark
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }
    }

    public void ToggleTheme() => SetTheme(!IsDarkTheme);
}