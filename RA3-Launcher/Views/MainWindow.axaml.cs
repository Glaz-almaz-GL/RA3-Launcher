using Huskui.Avalonia.Controls;
using Managers;
using RA3_Launcher.ViewModels;
using System;

namespace RA3_Launcher.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        GrowlsManager.Initialize(this);
        DialogsManager.Initialize(this, this);
        DataContextChanged += OnDataContextChanged; // Подписываемся на изменение DataContext
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.OpenSettingsRequested -= OnOpenSettingsRequested;
            viewModel.OpenSettingsRequested += OnOpenSettingsRequested;

            viewModel.OpenModsRequested -= OnOpenModsRequested;
            viewModel.OpenModsRequested += OnOpenModsRequested;
        }
    }

    private async void OnOpenSettingsRequested()
    {
        SettingsPage settingsPage = new();
        await settingsPage.ShowDialog(this); // Передаём текущее окно как родительское
    }

    private async void OnOpenModsRequested()
    {

    }
}
