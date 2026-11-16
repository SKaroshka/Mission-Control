using Avalonia.Controls;
using SmartTravelPlanner.ViewModels;

namespace SmartTravelPlanner.Views;

public partial class MapDialogWindow : Window
{
    private bool _canClose;

    public MapDialogWindow()
    {
        InitializeComponent();
    }

    public MapDialogWindow(MapDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.RequestClose = result =>
        {
            _canClose = true;
            Close(result);
        };
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (_canClose)
            return;
        
        if (DataContext is MapDialogViewModel vm)
        {
            e.Cancel = true;

            if (await vm.CanCloseAsync())
            {
                _canClose = true;
                Close(false);
            }
        }
    }

}