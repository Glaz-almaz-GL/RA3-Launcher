using Avalonia.Controls;
using RA3_Launcher.ViewModels;

namespace RA3_Launcher.Views;

public partial class MainView : UserControl
{
    public MainViewModel ViewModel { get; set; } = new();

    public MainView()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
