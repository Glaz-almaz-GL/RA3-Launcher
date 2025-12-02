using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Huskui.Avalonia.Controls;
using Items.Mod;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace RA3_Launcher.Toasts;

public partial class ModInfoToast : Toast
{
    public static readonly StyledProperty<Mod?> ModProperty =
        AvaloniaProperty.Register<ModInfoToast, Mod?>(nameof(Mod));

    public Mod? Mod
    {
        get => GetValue(ModProperty);
        set => SetValue(ModProperty, value); // ← НЕ трогайте DataContext!
    }

    public ModInfoToast()
    {
        InitializeComponent();
        DataContext = this; // ← важно: DataContext — сам тост
    }

    [RelayCommand]
    private void OpenWebsite() => OpenUri(Mod?.Website);

    [RelayCommand]
    private void OpenRepository() => OpenUri(Mod?.RepositoryUrl);

    private static void OpenUri(string? uri)
    {
        if (string.IsNullOrWhiteSpace(uri)) return;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = uri,
                UseShellExecute = true
            });
        }
        catch { /* ignore */ }
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Dismiss();
}