using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Managers.ModManagers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Items.Mod;

public partial class ModViewModel : ObservableObject
{
    public readonly ModMetadata Metadata;
    private readonly List<InstalledModVersion> _installedVersions = [];

    public string Name => Metadata.Name;
    public string? Description => Metadata.Description;
    public string? LatestVersion => Metadata.LatestVersion;

    public string? Author => Metadata.Author;
    public string? Category => Metadata.Category;
    public string? GameVersion => Metadata.GameVersion;

    [ObservableProperty]
    private bool _isInstalled;

    [ObservableProperty]
    private bool _isLatestVersionInstalled;

    [ObservableProperty]
    private string? _latestInstalledVersion;

    [ObservableProperty]
    private List<string> _installedLanguages = [];

    [ObservableProperty]
    private string? _selectedLanguage; // например, "Russian"

    [ObservableProperty]
    private IReadOnlyList<string> _availableLanguages = [];

    public ModViewModel(ModMetadata metadata)
    {
        Metadata = metadata;
        Reload();
    }

    public void Reload()
    {
        LoadInstalledVersions();

        ModVersionMetadata? latestVersion = Metadata.Versions?.FirstOrDefault(v => v.Version == Metadata.LatestVersion);
        if (latestVersion != null)
        {
            SetAvailableLanguages(ModInstallationManager.GetAvailableLanguagesForVersion(latestVersion));
        }

        UpdateStatus();
    }

    private void LoadInstalledVersions()
    {
        _installedVersions.Clear();
        IEnumerable<string> dirs = Directory.EnumerateDirectories(ModInstallationManager.ModsBasePath)
            .Where(dir => dir.EndsWith($" {Name}") || Path.GetFileName(dir).StartsWith($"{Name} "));

        foreach (string dir in dirs)
        {
            string dirName = Path.GetFileName(dir);
            if (dirName.Length > Name.Length && dirName[Name.Length] == ' ')
            {
                string version = dirName[(Name.Length + 1)..];
                _installedVersions.Add(new InstalledModVersion(version, dir));
            }
        }
    }

    public void SetAvailableLanguages(IReadOnlyList<string> languages)
    {
        AvailableLanguages = languages;
        SelectedLanguage = languages[0];
    }

    private void UpdateStatus()
    {
        InstalledModVersion? latestVersionDir = _installedVersions
            .FirstOrDefault(v => string.Equals(v.Version, LatestVersion, StringComparison.OrdinalIgnoreCase));

        if (latestVersionDir == null)
        {
            IsInstalled = false;
            LatestInstalledVersion = null;
            IsLatestVersionInstalled = false;
            InstalledLanguages = [];
            return;
        }

        // Получаем установленные языки в последней версии
        List<string> installedLangs = latestVersionDir.GetInstalledLanguages();
        InstalledLanguages = installedLangs;

        LatestInstalledVersion = LatestVersion;
        IsLatestVersionInstalled = true;

        // Проверяем: установлена ли версия И нужный язык
        bool hasSelectedLanguage = string.IsNullOrWhiteSpace(SelectedLanguage) ||
                                  installedLangs.Contains(SelectedLanguage, StringComparer.OrdinalIgnoreCase);

        IsInstalled = hasSelectedLanguage;
    }

    [RelayCommand]
    public async Task Download()
    {
        await ModInstallationManager.InstallModAsync(this, SelectedLanguage);
        Reload();
    }

    [RelayCommand]
    public async Task Delete()
    {
        await ModInstallationManager.UninstallModAsync(this);
        Reload();
    }

    public InstalledModInfo? ToInstalledModInfo()
    {
        if (string.IsNullOrWhiteSpace(LatestVersion))
        {
            return null;
        }

        string versionDir = Path.Combine(ModInstallationManager.ModsBasePath, $"{Name} {LatestVersion}");
        string skudefPath = Path.Combine(versionDir, $"{Name} {LatestVersion}.skudef");
        return new(Name, LatestVersion, skudefPath);
    }
}