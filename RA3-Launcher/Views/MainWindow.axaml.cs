using Huskui.Avalonia.Controls;
using Items.Mod;
using Managers;
using Managers.Github;
using RA3_Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        //List<Mod> mods = await GitHubModManager.GetModsAsync();

        //foreach (Mod mod in mods)
        //{
        //    Debug.WriteLine("--- Начало информации о моде ---");
        //    Debug.WriteLine($"Name: {mod.Name}");
        //    Debug.WriteLine($"Description: {mod.Description ?? "null"}");
        //    Debug.WriteLine($"CreationDate: {mod.CreationDate}");
        //    Debug.WriteLine($"Author: {mod.Author ?? "null"}");
        //    Debug.WriteLine($"Category: {mod.Category ?? "null"}");
        //    Debug.WriteLine($"GameVersion: {mod.GameVersion ?? "null"}");
        //    // Предполагается, что у Mod есть свойства Website, RepositoryUrl, Screenshots, TotalDownloads, IsFeatured
        //    Debug.WriteLine($"Website: {mod.Website ?? "null"}");
        //    Debug.WriteLine($"RepositoryUrl: {mod.RepositoryUrl ?? "null"}");
        //    Debug.WriteLine($"TotalDownloads: {mod.TotalDownloads}");
        //    Debug.WriteLine($"LastUpdated: {mod.LastUpdated}");
        //    Debug.WriteLine($"LatestVersion: {mod.LatestVersion ?? "null"}");
        //    Debug.WriteLine($"IsFeatured: {mod.IsFeatured}");

        //    // --- Логирование Main Files мода (из корня каталога мода) ---
        //    Debug.WriteLine($"Number of Main Files (from mod root): {mod.MainFiles.Count}");
        //    for (int i = 0; i < mod.MainFiles.Count; i++)
        //    {
        //        var fileInfo = mod.MainFiles[i];
        //        Debug.WriteLine($"  --- Main File {i}: {fileInfo.FileName} ---");
        //        Debug.WriteLine($"    DownloadUrl: {fileInfo.DownloadUrl}");
        //        Debug.WriteLine($"    Size: {fileInfo.Size} bytes");
        //        Debug.WriteLine($"    Checksum: {fileInfo.Checksum}");
        //        Debug.WriteLine($"    FileType: {fileInfo.FileType}");
        //        Debug.WriteLine($"    IsModMainFile: {fileInfo.IsModMainFile}"); // <-- Новый флаг
        //        Debug.WriteLine($"    IsCommonFile: {fileInfo.IsCommonFile}"); // <-- Должно быть false для файлов из корня мода
        //        Debug.WriteLine($"    IsVersionMainFile: {fileInfo.IsVersionMainFile}"); // <-- Должно быть false для файлов из корня мода
        //        Debug.WriteLine($"    LanguageCode: {fileInfo.LanguageCode ?? "null"}");
        //        Debug.WriteLine($"    LastModified: {fileInfo.LastModified}");
        //        Debug.WriteLine("  --- Конец информации о Main File ---");
        //    }
        //    // --- Конец логирования Main Files мода ---

        //    // Вывод списков
        //    Debug.WriteLine($"Screenshots: [{string.Join(", ", mod.Screenshots)}]");
        //    Debug.WriteLine($"AvailableLanguages: [{string.Join(", ", mod.AvailableLanguages)}]");
        //    Debug.WriteLine($"Number of Versions: {mod.Versions.Count}");

        //    // Логирование информации о каждой версии
        //    for (int i = 0; i < mod.Versions.Count; i++)
        //    {
        //        ModVersion version = mod.Versions[i];
        //        Debug.WriteLine($"--- Начало информации о версии {i} (Mod: {mod.Name}) ---");
        //        Debug.WriteLine($"  VersionNumber: {version.VersionNumber ?? "null"}");
        //        Debug.WriteLine($"  Changelog: {version.Changelog ?? "null"}");
        //        Debug.WriteLine($"  UpdateDate: {version.UpdateDate}");
        //        Debug.WriteLine($"  IsBeta: {version.IsBeta}");
        //        Debug.WriteLine($"  RequiredGameVersion: {version.RequiredGameVersion ?? "null"}");

        //        // Вывод списков для версии
        //        Debug.WriteLine($"  AvailableLanguages (for this version): [{string.Join(", ", version.AvailableLanguages)}]");

        //        // --- Логирование Version Main Files (из каталога версии, не из Languages) ---
        //        Debug.WriteLine($"  Number of Version Main Files (from version folder): {version.VersionMainFiles.Count}");
        //        for (int j = 0; j < version.VersionMainFiles.Count; j++)
        //        {
        //            var fileInfo = version.VersionMainFiles[j];
        //            Debug.WriteLine($"    --- Version Main File {j}: {fileInfo.FileName} ---");
        //            Debug.WriteLine($"      DownloadUrl: {fileInfo.DownloadUrl}");
        //            Debug.WriteLine($"      Size: {fileInfo.Size} bytes");
        //            Debug.WriteLine($"      Checksum: {fileInfo.Checksum}");
        //            Debug.WriteLine($"      FileType: {fileInfo.FileType}");
        //            Debug.WriteLine($"      IsModMainFile: {fileInfo.IsModMainFile}"); // <-- Должно быть false для файлов из каталога версии
        //            Debug.WriteLine($"      IsCommonFile: {fileInfo.IsCommonFile}"); // <-- Должно быть false для файлов из каталога версии (не Languages)
        //            Debug.WriteLine($"      IsVersionMainFile: {fileInfo.IsVersionMainFile}"); // <-- Должно быть true
        //            Debug.WriteLine($"      LanguageCode: {fileInfo.LanguageCode ?? "null"}");
        //            Debug.WriteLine($"      LastModified: {fileInfo.LastModified}");
        //            Debug.WriteLine("    --- Конец информации о Version Main File ---");
        //        }
        //        // --- Конец логирования Version Main Files ---

        //        // Логирование информации о *всех* файлах в версии (включая VersionMainFiles и Common Files)
        //        Debug.WriteLine($"  Number of Files in this version (All Files): {version.AllFiles.Count}");
        //        foreach (KeyValuePair<string, ModFileInfo> fileKvp in version.AllFiles)
        //        {
        //            ModFileInfo fileInfo = fileKvp.Value;
        //            Debug.WriteLine($"    --- Файл: {fileInfo.FileName} ---");
        //            Debug.WriteLine($"      DownloadUrl: {fileInfo.DownloadUrl}");
        //            Debug.WriteLine($"      Size: {fileInfo.Size} bytes");
        //            Debug.WriteLine($"      Checksum: {fileInfo.Checksum}");
        //            Debug.WriteLine($"      FileType: {fileInfo.FileType}");
        //            Debug.WriteLine($"      IsModMainFile: {fileInfo.IsModMainFile}"); // true для файлов из корня мода, добавленных в контексте версии (если такое возможно)
        //            Debug.WriteLine($"      IsCommonFile: {fileInfo.IsCommonFile}"); // true для файлов из Languages
        //            Debug.WriteLine($"      IsVersionMainFile: {fileInfo.IsVersionMainFile}"); // true для файлов из каталога версии (не Languages)
        //            Debug.WriteLine($"      LanguageCode: {fileInfo.LanguageCode ?? "null"}");
        //            Debug.WriteLine($"      LastModified: {fileInfo.LastModified}");
        //            Debug.WriteLine("    --- Конец информации о файле ---");
        //        }

        //        Debug.WriteLine($"--- Конец информации о версии {i} (Mod: {mod.Name}) ---");
        //    }

        //    Debug.WriteLine("--- Конец информации о моде ---");
        //}

        Content = new DownloadsPage();
    }
}
