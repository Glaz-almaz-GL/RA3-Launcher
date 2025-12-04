using Huskui.Avalonia.Controls;
using Items.Mod;
using Managers.AvaloniaManagers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Managers.ModManagers
{
    /// <summary>
    /// Менеджер установки и удаления модов.
    /// Поддерживает установку конкретной версии и корректную обработку языков.
    /// Совместим с ModViewModel и новой метадатой.
    /// </summary>
    public static class ModInstallationManager
    {
        private static readonly HttpClient _httpClient = new(new HttpClientHandler()
        {
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
        })
        {
            Timeout = TimeSpan.FromHours(2) // или другое разумное значение, например: 2 часа
        };

        /// <summary>
        /// Базовый каталог модов: %USERPROFILE%/Documents/Red Alert 3/Mods
        /// </summary>
        public static string ModsBasePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Red Alert 3", "Mods");

        /// <summary>
        /// Устанавливает мод через ModViewModel, используя последнюю версию из метаданных.
        /// </summary>
        public static async Task InstallModAsync(ModViewModel viewModel, string? languageCode = null)
        {
            if (viewModel == null)
            {
                return;
            }

            string? latestVersion = viewModel.LatestVersion;
            if (string.IsNullOrWhiteSpace(latestVersion))
            {
                GrowlsManager.ShowWarningMsg("Нет доступной версии для установки.", "Установка невозможна");
                return;
            }

            await InstallModVersionAsync(viewModel.Metadata, latestVersion, languageCode);
        }
        /// <summary>
        /// Удаляет мод через ModViewModel, удаляя последнюю установленную версию.
        /// </summary>
        public static async Task UninstallModAsync(ModViewModel viewModel)
        {
            if (viewModel == null || string.IsNullOrWhiteSpace(viewModel.LatestInstalledVersion))
            {
                GrowlsManager.ShowWarningMsg("Нет установленных версий для удаления.", "Удаление невозможно");
                return;
            }

            await UninstallModVersionAsync(viewModel.Name, viewModel.LatestInstalledVersion);
        }

        /// <summary>
        /// Устанавливает указанную версию мода на основе ModMetadata.
        /// </summary>
        public static async Task InstallModVersionAsync(
    ModMetadata mod,
    string versionNumber,
    string? languageCode = null) // ← НОВЫЙ ПАРАМЕТР
        {
            ModVersionMetadata? targetVersion = mod.Versions?.FirstOrDefault(v => v.Version == versionNumber);
            if (targetVersion == null)
            {
                GrowlsManager.ShowWarningMsg($"Версия {versionNumber} не найдена для мода «{mod.Name}».", "Установка невозможна");
                return;
            }

            string versionDir = Path.Combine(ModsBasePath, $"{mod.Name} {versionNumber}");
            string skudefPath = Path.Combine(versionDir, $"{mod.Name} {versionNumber} {languageCode}.skudef");

            try
            {
                GrowlItem? progressGrowl = GrowlsManager.ShowProgressInfoMsg(
                    "Установка мода",
                    $"Начало установки «{mod.Name}» версии {versionNumber}",
                    progress: 0
                );

                string commonModDir = Path.Combine(ModsBasePath, mod.Name);

                // 1. Общие файлы
                if (mod.MainFiles?.Count > 0)
                {
                    Directory.CreateDirectory(commonModDir);
                    await InstallFilesAsync(mod.MainFiles, commonModDir, progressGrowl);
                }

                // 2. Файлы версии
                Directory.CreateDirectory(versionDir);
                await InstallFilesAsync(targetVersion.Files, versionDir, progressGrowl);

                // 3. Если выбран язык — скачиваем языковой файл
                ModFileInfo? languageFile = null;
                if (!string.IsNullOrWhiteSpace(languageCode))
                {
                    languageFile = targetVersion.Files
                        .FirstOrDefault(f => string.Equals(f.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

                    if (languageFile != null)
                    {
                        if (progressGrowl != null)
                        {
                            Avalonia.Threading.Dispatcher.UIThread.Post(() => progressGrowl.Content = $"Установка языка: {languageCode}");
                        }

                        await InstallFilesAsync([languageFile], versionDir, progressGrowl);
                    }
                    else
                    {
                        GrowlsManager.ShowWarningMsg($"Язык «{languageCode}» не найден для этой версии.", "Язык недоступен");
                    }
                }

                // 4. Генерируем .skudef с учётом языка
                await GenerateSkudefAsync(mod, targetVersion, skudefPath, languageFile);

                GrowlsManager.ShowSuccessMsg(
                    $"Мод «{mod.Name}» версии {versionNumber}" +
                    (!string.IsNullOrWhiteSpace(languageCode) ? $" ({languageCode})" : "") +
                    " установлен.",
                    "Установка завершена"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ModInstallationManager] Ошибка установки: {ex}");
                GrowlsManager.ShowErrorMsg(ex, $"Не удалось установить мод «{mod.Name}»", false);
            }
        }

        /// <summary>
        /// Удаляет указанную версию мода.
        /// </summary>
        public static async Task UninstallModVersionAsync(string modName, string versionNumber)
        {
            string versionDir = Path.Combine(ModsBasePath, $"{modName} {versionNumber}");

            try
            {
                if (Directory.Exists(versionDir))
                {
                    Directory.Delete(versionDir, recursive: true);
                    Debug.WriteLine($"[ModInstallationManager] Удалена версия: {versionDir}");
                }

                // Удаляем общую папку, если версий больше нет
                await CleanupCommonFolderIfUnused(modName);

                GrowlsManager.ShowSuccessMsg($"Версия {versionNumber} мода «{modName}» удалена.", "Удаление завершено");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ModInstallationManager] Ошибка удаления: {ex}");
                GrowlsManager.ShowErrorMsg(ex, $"Не удалось удалить версию мода «{modName}»", false);
            }
        }

        /// <summary>
        /// Устанавливает файлы (только .big и .lyi), обрабатывая языковые файлы.
        /// </summary>
        private static async Task InstallFilesAsync(
             IEnumerable<ModFileInfo> files,
             string targetDir,
             GrowlItem? progressGrowl = null)
        {
            if (files == null)
            {
                return;
            }

            List<ModFileInfo> validFiles = [.. files.Where(f => f.FileType is ModFileType.Big or ModFileType.Lyi)];
            if (validFiles.Count == 0)
            {
                return;
            }

            for (int i = 0; i < validFiles.Count; i++)
            {
                ModFileInfo file = validFiles[i];
                string fileName = file.LanguageCode != null ? Path.GetFileName(file.FileName) : file.FileName;
                string destPath = Path.Combine(targetDir, fileName);

                if (File.Exists(destPath))
                {
                    Debug.WriteLine($"[ModInstallationManager] Файл уже существует: {fileName}");
                    if (progressGrowl != null)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() => progressGrowl.Content = $"Пропущен (уже существует): {fileName}");
                    }
                    continue;
                }

                if (string.IsNullOrWhiteSpace(file.DownloadUrl))
                {
                    Debug.WriteLine($"[ModInstallationManager] Пропущен файл без URL: {fileName}");
                    continue;
                }

                await DownloadFileWithRetryAsync(file.DownloadUrl, destPath, fileName, progressGrowl);
            }

            // Завершение
            if (progressGrowl != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    progressGrowl.Progress = 1.0;
                    progressGrowl.Content = "Все файлы успешно установлены!";
                    progressGrowl.IsProgressBarVisible = false;
                });
            }
        }

        /// <summary>
        /// Генерирует .skudef файл для запуска RA3 с модом.
        /// </summary>
        private static async Task GenerateSkudefAsync(
            ModMetadata mod,
            ModVersionMetadata version,
            string skudefPath,
            ModFileInfo? languageFile = null)
        {
            List<string> lines =
            [
                $"mod-game {mod.GameVersion}"
            ];

            // Общие файлы
            if (mod.MainFiles != null)
            {
                foreach (ModFileInfo file in mod.MainFiles)
                {
                    if (file.FileType is not (ModFileType.Big or ModFileType.Lyi))
                    {
                        continue;
                    }

                    string relPath = Path.Combine("..", mod.Name, file.FileName).Replace('\\', '/');
                    lines.Add($"add-big {relPath}");
                }
            }

            // Файлы версии
            foreach (ModFileInfo file in version.Files)
            {
                if (file.FileType is not (ModFileType.Big or ModFileType.Lyi))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(file.LanguageCode))
                {
                    continue; // языки обрабатываем отдельно
                }

                string fileName = file.FileName;
                lines.Add($"add-big ./{fileName}");
            }

            // Языковой файл (если есть)
            if (languageFile != null)
            {
                string langFileName = Path.GetFileName(languageFile.FileName);
                lines.Add($"add-big ./{langFileName}");
            }

            await File.WriteAllTextAsync(skudefPath, string.Join(Environment.NewLine, lines), Encoding.UTF8);
            Debug.WriteLine($"[ModInstallationManager] Сгенерирован .skudef: {skudefPath}");
        }

        /// <summary>
        /// Удаляет общую папку, если ни одна версия мода не установлена.
        /// </summary>
        private static async Task CleanupCommonFolderIfUnused(string modName)
        {
            string commonDir = Path.Combine(ModsBasePath, modName);
            if (!Directory.Exists(commonDir))
            {
                return;
            }

            bool hasAnyVersion = Directory.EnumerateDirectories(ModsBasePath)
                .Any(dir => Path.GetFileName(dir).StartsWith($"{modName} ", StringComparison.OrdinalIgnoreCase));

            if (!hasAnyVersion)
            {
                Directory.Delete(commonDir, recursive: true);
                Debug.WriteLine($"[ModInstallationManager] Удалена общая папка (нет активных версий): {commonDir}");
            }
        }

        private static async Task DownloadFileWithRetryAsync(
            string url,
            string filePath,
            string fileName,
            GrowlItem? growlItem,
            int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await DownloadFileAsync(url, filePath, fileName, growlItem);
                    return;
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    Debug.WriteLine($"Попытка {attempt} не удалась: {ex.Message}. Повтор...");
                    if (growlItem != null)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() => growlItem.Content = $"Попытка {attempt} не удалась: {ex.Message}\nПовтор через {2 * attempt} сек...");
                    }
                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
                }
            }

            // Последняя попытка — проброс исключения
            await DownloadFileAsync(url, filePath, fileName, growlItem);
        }

        /// <summary>
        /// Асинхронная загрузка файла по URL.
        /// </summary>
        private static async Task DownloadFileAsync(
            string url,
            string filePath,
            string fileName,
            GrowlItem? growlItem,
            CancellationToken cancellationToken = default)
        {
            Debug.WriteLine($"[ModInstallationManager] Загрузка: {url}");
            using HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
            await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            const int bufferSize = 81920;
            byte[] buffer = new byte[bufferSize];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;

                if (growlItem != null)
                {
                    string downloaded = FormatBytes(totalRead);
                    string total = totalBytes >= 0 ? FormatBytes(totalBytes) : "???";

                    double progressPercent = ToPercent(totalRead, totalBytes);

                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        growlItem.Progress = Math.Min(100.0, progressPercent); // ← теперь от 0 до 100
                        growlItem.Content = $"Скачивание: {fileName}\n{downloaded} из {total}";
                        growlItem.IsProgressBarVisible = true;
                    });
                }
            }

            Debug.WriteLine($"[ModInstallationManager] Сохранено: {filePath}");
        }

        private static double ToPercent(long current, long total)
        {
            return total > 0 ? Math.Min(100.0, current * 100.0 / total) : 0.0;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
            {
                return $"{bytes} B";
            }
            else if (bytes < 1024 * 1024)
            {
                return $"{bytes / 1024.0:F1} KB";
            }
            else if (bytes < 1024 * 1024 * 1024)
            {
                return $"{bytes / (1024.0 * 1024.0):F1} MB";
            }
            else
            {
                return $"{bytes / (1024.0 * 1024.0 * 1024.0):F1} GB";
            }
        }

        public static IReadOnlyList<string> GetAvailableLanguagesForVersion(ModVersionMetadata version)
        {
            if (version?.Files == null)
            {
                return [];
            }

            List<string> languages = [.. version.Files
                .Where(f => !string.IsNullOrWhiteSpace(f.LanguageCode))
                .Select(f => f.LanguageCode!)
                .Distinct(StringComparer.OrdinalIgnoreCase)];

            return languages;
        }
    }
}