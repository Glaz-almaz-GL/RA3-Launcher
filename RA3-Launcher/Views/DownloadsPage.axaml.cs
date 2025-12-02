using Avalonia.Controls;
using Huskui.Avalonia.Controls;
using Items.Mod;
using RA3_Launcher.Toasts;
using ViewModels;

namespace RA3_Launcher.Views;

public partial class DownloadsPage : UserControl
{
    private DownloadViewModel _viewModel = new();

    public DownloadsPage()
    {
        InitializeComponent();
        DataContext = _viewModel;
        _ = _viewModel.LoadModsAsync();
    }

    private AppWindow? GetAppWindow() => TopLevel.GetTopLevel(this) as AppWindow;

    private void ModInfo_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        // Получаем Mod из DataContext кликнутого элемента
        if (sender is Control control && control.DataContext is Mod mod)
        {
            var appWindow = GetAppWindow();
            if (appWindow == null) return;

            var toast = new ModInfoToast
            {
                Mod = mod // ← Устанавливаем свойство Mod
            };

            appWindow.PopToast(toast);
        }
    }
}