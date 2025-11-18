using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using RA3_Launcher.Items;
using RA3_Launcher.Managers;
using RA3_Launcher.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RA3_Launcher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public event Action? OpenSettingsRequested;

    [JsonIgnore]
    public string CurrentVersion { get; set; } = "Current Version";

    [JsonIgnore]
    public List<ModInfo> AvailableMods { get; set; } = ModManager.GetModsFromDocuments();

    [JsonIgnore]
    public ModInfo? SelectedMod { get; set; } = null;

    [RelayCommand]
    public async Task LaunchGameCommand()
    {
        string gamePath = SettingsManager.CurrentSettings.GamePath;

        if (!string.IsNullOrWhiteSpace(gamePath) && File.Exists(gamePath))
        {
            List<string> launchOptions = [.. SettingsManager.CurrentSettings.LaunchOptions];
            launchOptions.Add("-runver 1.12");

            if (SelectedMod != null)
            {
                // Убираем дубликаты опции -modconfig, если она уже есть, и добавляем новую
                launchOptions.RemoveAll(opt => opt.StartsWith("-modconfig "));
                launchOptions.Add($"-modconfig \"{SelectedMod.ModPath}\""); // Заключаем путь в кавычки на случай пробелов
            }

            ProcessStartInfo ra3 = new()
            {
                FileName = gamePath,
                Arguments = string.Join(" ", launchOptions),
                UseShellExecute = false
            };

            Process.Start(ra3);
        }
        else if (!File.Exists(gamePath))
        {
            GrowlsManager.ShowErrorMsg("Укажите путь до ra3.exe в настройках.");
        }
        else
        {
            GrowlsManager.ShowErrorMsg("Укажите путь до ra3.exe в настройках.");
        }
    }

    [RelayCommand]
    public void OpenSettingsCommand()
    {
        OpenSettingsRequested?.Invoke();
    }

    [RelayCommand]
    public void OpenModsCommand()
    {

    }
}
