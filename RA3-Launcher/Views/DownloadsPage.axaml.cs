using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Huskui.Avalonia.Controls;
using Items.Mod;
using RA3_Launcher.Toasts;
using System.Diagnostics;
using ViewModels;

namespace RA3_Launcher.Views;

public partial class DownloadsPage : UserControl
{
    public readonly DownloadViewModel ViewModel = new();
    private bool _hasLoaded = false;

    public DownloadsPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += LoadIfNeeded;
    }

    public void LoadIfNeeded(object? sender, RoutedEventArgs e)
    {
        if (!_hasLoaded)
        {
            _ = ViewModel.LoadModsAsync();
            _hasLoaded = true;
        }
    }

    private AppWindow? GetAppWindow()
    {
        return TopLevel.GetTopLevel(this) as AppWindow;
    }

    private void InteractiveElement_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Debug.WriteLine("IGNORE CLICK");
        e.Handled = true;
    }

    private void ModInfo_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        // Получаем Mod из DataContext кликнутого элемента
        if (sender is Control control && control.DataContext is ModViewModel mod)
        {
            AppWindow? appWindow = GetAppWindow();
            if (appWindow == null)
            {
                return;
            }

            ModInfoToast toast = new()
            {
                Mod = mod.Metadata
            };

            appWindow.PopToast(toast);
        }
    }
}