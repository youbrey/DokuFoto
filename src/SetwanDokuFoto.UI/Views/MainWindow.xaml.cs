using System.Windows;
using SetwanDokuFoto.UI.ViewModels;

namespace SetwanDokuFoto.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
