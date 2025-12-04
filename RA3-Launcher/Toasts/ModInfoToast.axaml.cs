using Avalonia;
using Avalonia.Markup.Xaml;
using Huskui.Avalonia.Controls;
using Items.Mod;
using RA3_Launcher.ViewModels;

namespace RA3_Launcher.Toasts;

public partial class ModInfoToast : Toast
{
    public static readonly StyledProperty<ModMetadata?> ModProperty =
        AvaloniaProperty.Register<ModInfoToast, ModMetadata?>(nameof(Mod));

    public ModMetadata? Mod
    {
        get => GetValue(ModProperty);
        set
        {
            SetValue(ModProperty, value);
            if (value != null)
            {
                DataContext = new ModInfoToastViewModel { Mod = value };
            }
        }
    }

    public ModInfoToast()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}