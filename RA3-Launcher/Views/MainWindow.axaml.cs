using Huskui.Avalonia.Controls;
using RA3_Launcher.Managers;
using RA3_Launcher.ViewModels;
using System;

namespace RA3_Launcher.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        GrowlsManager.Initialize(this);
        DataContextChanged += OnDataContextChanged; // Подписываемся на изменение DataContext
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.OpenSettingsRequested -= OnOpenSettingsRequested;
            viewModel.OpenSettingsRequested += OnOpenSettingsRequested;
        }
    }

    private async void OnOpenSettingsRequested()
    {
        SettingsPage dialog = new();
        await dialog.ShowDialog(this); // Передаём текущее окно как родительское
    }
}
