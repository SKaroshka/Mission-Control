using System;
using System.Collections.Specialized;
//
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SmartTravelPlanner.Controls;

public class LockedCarouselPage : CarouselPage
{
    public static readonly StyledProperty<bool> IsArrowNavigationEnabledProperty =
        AvaloniaProperty.Register<LockedCarouselPage, bool>(
            nameof(IsArrowNavigationEnabled),
            defaultValue: false);

    public bool IsArrowNavigationEnabled
    {
        get => GetValue(IsArrowNavigationEnabledProperty);
        set => SetValue(IsArrowNavigationEnabledProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(CarouselPage);

    public LockedCarouselPage()
    {
        AddHandler(KeyDownEvent, OnTunnelKeyDown, RoutingStrategies.Tunnel);

        if (Pages is INotifyCollectionChanged notifyPages)
            notifyPages.CollectionChanged += OnPagesCollectionChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (Pages is not null)
        {
            foreach (var page in Pages)
                AttachPageHandler(page);
        }
    }

    private void OnPagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (Page page in e.OldItems)
                page.RemoveHandler(KeyDownEvent, OnPageBubbleKeyDown);
        }

        if (e.NewItems is not null)
        {
            foreach (Page page in e.NewItems)
                AttachPageHandler(page);
        }
    }

    private void AttachPageHandler(Page page)
    {
        page.RemoveHandler(KeyDownEvent, OnPageBubbleKeyDown);
        page.AddHandler(KeyDownEvent, OnPageBubbleKeyDown, RoutingStrategies.Bubble);
    }

    private static bool IsNavigationKey(Key key) =>
        key is Key.Left or Key.Right or Key.Up or Key.Down or Key.PageUp or Key.PageDown;

    private void OnTunnelKeyDown(object? sender, KeyEventArgs e)
    {
        if (IsArrowNavigationEnabled || !IsNavigationKey(e.Key))
            return;

        if (e.Source is TextBox)
            return;

        e.Handled = true;
    }

    private void OnPageBubbleKeyDown(object? sender, KeyEventArgs e)
    {
        if (IsArrowNavigationEnabled || !IsNavigationKey(e.Key))
            return;

        e.Handled = true;
    }
}