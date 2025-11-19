using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Items;
using Managers;
using RA3_Launcher.Managers;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ViewModels;

namespace RA3_Launcher.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        [JsonIgnore]
        [ObservableProperty]
        private string _launchOptions = SettingsManager.CurrentSettings.LaunchOptions ?? string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private string _gamePath = SettingsManager.CurrentSettings.GamePath;

        [JsonIgnore]
        [ObservableProperty]
        private bool _checkUpdatesForMods = SettingsManager.CurrentSettings.CheckUpdatesForMods;

        [JsonIgnore]
        [ObservableProperty]
        private bool _checkUpdatesForApp = SettingsManager.CurrentSettings.CheckUpdatesForApp;

        [RelayCommand]
        private async Task BrowseGamePath()
        {
            var file = await DialogsManager.ShowOpenSingleFileDialogAsync("Выберите исполняемый файл RA3", ["*.exe"]);

            if (file != null)
            {
                string? filePath = file.TryGetLocalPath();

                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    GamePath = filePath;
                }
                else if (file.Path.AbsolutePath != null)
                {
                    GamePath = file.Path.AbsolutePath;
                }
            }
        }

        [RelayCommand]
        private void Apply4GBPatch()
        {
            InstallPatchManager.Install4GBPatch();
        }

        [RelayCommand]
        private void FixRegistry()
        {
            RegistryManager.FixRegistry();
        }

        [RelayCommand]
        private async Task InstallBattleNet()
        {
            try
            {
                Debug.WriteLine($"Открытие ссылки в браузере: {FilePaths.RA3BattleNetUrl}");
                // Используем Avalonia PlatformManager для открытия URI
                Process.Start(new ProcessStartInfo(FilePaths.RA3BattleNetUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при открытии ссылки в браузере: {ex.Message}");
                GrowlsManager.ShowErrorMsg($"Не удалось открыть браузер: {ex.Message}");
            }
        }

        [RelayCommand]
        private void InstallCnCOnline()
        {
            try
            {
                Debug.WriteLine($"Открытие ссылки в браузере: {FilePaths.RA3CnCUrl}");
                // Используем Avalonia PlatformManager для открытия URI
                Process.Start(new ProcessStartInfo(FilePaths.RA3CnCUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при открытии ссылки в браузере: {ex.Message}");
                GrowlsManager.ShowErrorMsg($"Не удалось открыть браузер: {ex.Message}");
            }
        }

        [RelayCommand]
        private void InstallRadminVPN()
        {
            try
            {
                Debug.WriteLine($"Открытие ссылки в браузере: {FilePaths.RadminVpnUrl}");
                // Используем Avalonia PlatformManager для открытия URI
                Process.Start(new ProcessStartInfo(FilePaths.RadminVpnUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при открытии ссылки в браузере: {ex.Message}");
                GrowlsManager.ShowErrorMsg($"Не удалось открыть браузер: {ex.Message}");
            }
        }

        [RelayCommand]
        private void GenerateCDKey()
        {
            try
            {
                string cdKey = InstallPatchManager.GenerateCDKey();
                bool success = InstallPatchManager.ApplyCDKey(cdKey);

                if (success)
                {
                    GrowlsManager.ShowInfoMsg($"Ваш новый cd-ключ: {cdKey}");
                }
            }
            catch (Exception ex)
            {
                GrowlsManager.ShowErrorMsg(ex, null);
            }
        }

        [RelayCommand]
        private void SaveSettings()
        {
            SettingsItem settings = new(GamePath, LaunchOptions, CheckUpdatesForMods, CheckUpdatesForApp);
            SettingsManager.SaveSettings(settings);
        }
    }
}