using Avalonia.Controls;
using Huskui.Avalonia.Controls;
using Managers;
using RA3_Launcher.ViewModels;
using System;

namespace RA3_Launcher.Views;

public partial class SettingsPage : AppWindow
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }
}