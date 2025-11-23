using Huskui.Avalonia.Controls;
using Managers;
using Managers.Github;
using RA3_Launcher.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;

namespace RA3_Launcher.Views;

public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();
        GrowlsManager.Initialize(this);
        DialogsManager.Initialize(this, this);
        GitHubModManager.Initialize();
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
        var mods = await GitHubModManager.GetModsAsync();

        foreach (var mod in mods)
        {
            Debug.WriteLine("--- Начало информации о моде ---");
            Debug.WriteLine($"Name: {mod.Name}");
            Debug.WriteLine($"Description: {mod.Description ?? "null"}"); // Обработка потенциального null
            Debug.WriteLine($"CreationDate: {mod.CreationDate}");
            Debug.WriteLine($"Author: {mod.Author ?? "null"}");
            Debug.WriteLine($"Category: {mod.Category ?? "null"}");
            Debug.WriteLine($"GameVersion: {mod.GameVersion ?? "null"}");
            // Вывод списка зависимостей как строки
            Debug.WriteLine($"Dependencies: [{string.Join(", ", mod.Dependencies)}]");
            Debug.WriteLine($"DownloadCount: {mod.DownloadCount}");
            Debug.WriteLine($"LastUpdated: {mod.LastUpdated}");
            Debug.WriteLine($"LatestVersion: {mod.LatestVersion ?? "null"}");
            // Вывод списка доступных языков как строки
            Debug.WriteLine($"AvailableLanguages: [{string.Join(", ", mod.AvailableLanguages)}]");
            Debug.WriteLine($"Number of Versions: {mod.Versions.Count}");

            // Логирование информации о каждой версии
            for (int i = 0; i < mod.Versions.Count; i++)
            {
                var version = mod.Versions[i];
                Debug.WriteLine($"--- Начало информации о версии {i} (Mod: {mod.Name}) ---");
                Debug.WriteLine($"  VersionNumber: {version.VersionNumber ?? "null"}");
                Debug.WriteLine($"  Changelog: {version.Changelog ?? "null"}");
                Debug.WriteLine($"  UpdateDate: {version.UpdateDate}");
                Debug.WriteLine($"  ModSize: {version.ModSize}");
                Debug.WriteLine($"  DownloadUrl: {version.DownloadUrl ?? "null"}");
                Debug.WriteLine($"  Checksum: {version.Checksum ?? "null"}");
                Debug.WriteLine($"  IsBeta: {version.IsBeta}");
                Debug.WriteLine($"  RequiredGameVersion: {version.RequiredGameVersion ?? "null"}");
                // Вывод списка доступных языков для версии как строки
                Debug.WriteLine($"  AvailableLanguages (for this version): [{string.Join(", ", version.AvailableLanguages)}]");
                // Вывод словаря языковых файлов как строки (ключ=значение)
                var langFilesList = version.LanguageFiles.Select(kvp => $"{kvp.Key}={kvp.Value}");
                Debug.WriteLine($"  LanguageFiles: {{{string.Join(", ", langFilesList)}}}");
                Debug.WriteLine($"  MainModFile: {version.MainModFile ?? "null"}");
                // Вывод списка общих файлов мода как строки
                Debug.WriteLine($"  CommonModFiles: [{string.Join(", ", version.CommonModFiles)}]");
                Debug.WriteLine($"--- Конец информации о версии {i} (Mod: {mod.Name}) ---");
            }

            Debug.WriteLine("--- Конец информации о моде ---");
        }
        //Content = new DownloadsPage();
    }
}
