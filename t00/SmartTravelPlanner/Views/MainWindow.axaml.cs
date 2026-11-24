using System;
using Avalonia.Controls;
using SmartTravelPlanner.ViewModels;

namespace SmartTravelPlanner.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainViewModel vm)
        {
            vm.ExitAction = Close;
        }
    }
}