using Huskui.Avalonia.Controls;
using RA3_Launcher.ViewModels;

namespace RA3_Launcher.Views;

public partial class SettingsPage : AppWindow
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }
}