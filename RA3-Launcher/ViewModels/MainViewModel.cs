using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Items.Mod;
using Managers.AvaloniaManagers;
using RA3_Launcher.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ViewModels;

namespace RA3_Launcher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public event Action? OpenSettingsRequested;
    public event Action? OpenModsRequested;

    public string CurrentVersion { get; set; } = "Current Version";

    [ObservableProperty]
    private ObservableCollection<InstalledModInfo> _installedMods = [];

    public InstalledModInfo? SelectedMod { get; set; } = null;

    public MainViewModel()
    {
        InstalledMods = new ObservableCollection<InstalledModInfo>(InstalledModsManager.InstalledMods);
    }

    [RelayCommand]
    public async Task LaunchGameCommand()
    {
        string gamePath = SettingsManager.CurrentSettings.GamePath;

        if (!string.IsNullOrWhiteSpace(gamePath) && File.Exists(gamePath))
        {
            string launchOptions = SettingsManager.CurrentSettings.LaunchOptions ?? string.Empty;

            // Убедимся, что опция -runver 1.12 не дублируется
            const string runverOption = "-runver 1.12";
            string[] parts = launchOptions.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            List<string> optionsList = [.. parts];

            // Удаляем существующую опцию -runver, если есть
            optionsList.RemoveAll(opt => opt.StartsWith("-runver"));

            // Добавляем нужную версию
            optionsList.Add(runverOption);

            if (SelectedMod != null)
            {
                // Удаляем все вхождения опции -modconfig
                optionsList.RemoveAll(opt => opt.StartsWith("-modconfig"));

                // Добавляем новую опцию -modconfig с кавычками
                optionsList.Add($"-modconfig \"{SelectedMod.ModPath}\"");
            }

            // Формируем итоговую строку аргументов
            launchOptions = string.Join(" ", optionsList);

            ProcessStartInfo ra3 = new()
            {
                FileName = gamePath,
                Arguments = launchOptions,
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
    private void RefreshInstalledMods()
    {
        InstalledMods.Clear();
        foreach (InstalledModInfo mod in InstalledModsManager.InstalledMods)
        {
            InstalledMods.Add(mod);
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
        OpenModsRequested?.Invoke();
    }
}
