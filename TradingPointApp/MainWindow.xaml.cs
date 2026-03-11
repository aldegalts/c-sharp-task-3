using System.Windows;
using TradingPointApp.ViewModels;

namespace TradingPointApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SimulationCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.CanvasWidth = e.NewSize.Width;
            vm.CanvasHeight = e.NewSize.Height;
        }
    }
}
