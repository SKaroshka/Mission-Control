using Avalonia.Controls;
using SmartTravelPlanner.ViewModels;

namespace SmartTravelPlanner.Views;

public partial class ConfirmationDialogWindow : Window
{
    public ConfirmationDialogWindow()
    {
        InitializeComponent();
    }

    public ConfirmationDialogWindow(ConfirmationDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.RequestClose = result => Close(result);
    }

}