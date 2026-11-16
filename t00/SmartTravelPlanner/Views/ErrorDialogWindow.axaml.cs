using Avalonia.Controls;
using SmartTravelPlanner.ViewModels;

namespace SmartTravelPlanner.Views;

public partial class ErrorDialogWindow : Window
{
    public ErrorDialogWindow()
    {
        InitializeComponent();
    }

    public ErrorDialogWindow(ErrorDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.RequestClose = result => Close(result);
    }

}