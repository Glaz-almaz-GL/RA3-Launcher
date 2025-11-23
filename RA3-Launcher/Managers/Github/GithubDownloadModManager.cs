using Items.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Managers.Github
{
    public static partial class GitHubModManager
    {
        /// <summary>
        /// Скачивает файлы для указанной версии мода на заданном языке и создаёт skudef файл для этой версии.
        /// Файлы версии (основной и языковой) сохраняются в папку версии (versionDownloadDirectory).
        /// Общие файлы мода сохраняются в общую папку мода (modDownloadDirectory), если их нет или они изменились.
        /// Папка мода (modDownloadDirectory) и папка версии (versionDownloadDirectory) находятся на одном уровне.
        /// </summary>
        /// <param name="mod">Объект мода.</param>
        /// <param name="versionNumber">Номер версии (например, "1.0").</param>
        /// <param name="language">Язык (например, "English", "Russian").</param>
        /// <param name="modDownloadDirectory">Путь к общей директории мода (например, "Downloads/ExampleMod").</param>
        /// <param name="versionDownloadDirectory">Путь к директории конкретной версии (например, "Downloads/ExampleMod_v2.1").</param>
        /// <param name="requestedModGame">Значение для строки 'mod-game RequestedModGame' в skudef файле.</param>
        /// <returns>Путь к созданному skudef файлу, или null в случае ошибки.</returns>
        public static async Task<string?> DownloadVersionLanguageFilesAndCreateSkudefAsync(Mod mod, string versionNumber, string language, string modDownloadDirectory, string versionDownloadDirectory, string requestedModGame)
        {
            if (string.IsNullOrEmpty(language))
            {
                throw new ArgumentException("Language cannot be null or empty.", nameof(language));
            }

            ModVersion? version = mod.Versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
            if (version == null)
            {
                Console.WriteLine($"Version {versionNumber} not found for mod {mod.Name}.");
                return null;
            }

            if (!version.AvailableLanguages.Contains(language, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Language '{language}' is not available for version {versionNumber} of mod {mod.Name}.");
                return null;
            }

            // Создаём директории для загрузки, если они не существуют
            Directory.CreateDirectory(modDownloadDirectory);
            Directory.CreateDirectory(versionDownloadDirectory);

            List<string> listOfBigFilesForSkudef = [];

            // 1. Скачиваем основной файл версии (MainModFile), если он есть
            if (!string.IsNullOrEmpty(version.MainModFile))
            {
                string originalFileName = Path.GetFileName(new Uri(version.MainModFile).LocalPath);
                string fileExtension = Path.GetExtension(originalFileName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
                string fileNameWithVersion = $"{fileNameWithoutExtension}_v{versionNumber}{fileExtension}";
                string filePath = Path.Combine(versionDownloadDirectory, fileNameWithVersion);

                await DownloadFileAsync(version.MainModFile, filePath);
                Console.WriteLine($"Downloaded main version file: {filePath}");

                // Добавляем имя файла (только имя, не полный путь) в список для skudef
                // skudef будет в versionDownloadDirectory, основной файл тоже, поэтому просто имя.
                listOfBigFilesForSkudef.Add(fileNameWithVersion);
            }

            // 2. Скачиваем общие файлы мода (CommonModFiles) в общую папку и добавляем в список
            foreach (string commonFileUrl in version.CommonModFiles)
            {
                string originalFileName = Path.GetFileName(new Uri(commonFileUrl).LocalPath);
                string targetFilePath = Path.Combine(modDownloadDirectory, originalFileName); // Сохраняем в общую папку без суффикса версии

                if (!File.Exists(targetFilePath) || !await IsFileUpToDateAsync(commonFileUrl, targetFilePath))
                {
                    await DownloadFileAsync(commonFileUrl, targetFilePath);
                    Console.WriteLine($"Downloaded/Updated common mod file: {targetFilePath}");
                }
                else
                {
                    Console.WriteLine($"Common mod file already exists and is up-to-date: {targetFilePath}");
                }

                // Добавляем относительный путь к файлу из папки версии к общей папке
                // Например, если versionDownloadDirectory = "Downloads/ExampleMod_v2.1" и modDownloadDirectory = "Downloads/ExampleMod"
                // Тогда путь к файлу из папки версии будет "../ExampleMod/имя_файла"
                string relativePathToCommonFile = Path.Combine("..", Path.GetFileName(modDownloadDirectory), originalFileName);
                // Убираем лишние разделители и нормализуем путь (например, на Windows может быть \)
                relativePathToCommonFile = relativePathToCommonFile.Replace('\\', '/');
                listOfBigFilesForSkudef.Add(relativePathToCommonFile);
            }

            // 3. Скачиваем языковой файл для указанного языка
            if (version.LanguageFiles.FirstOrDefault(kvp => string.Equals(kvp.Key, language, StringComparison.OrdinalIgnoreCase)).Value != null)
            {
                string urlToDownload = version.LanguageFiles.FirstOrDefault(kvp => string.Equals(kvp.Key, language, StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(urlToDownload))
                {
                    string originalFileName = Path.GetFileName(new Uri(urlToDownload).LocalPath);
                    string fileExtension = Path.GetExtension(originalFileName);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
                    // Пример: Russian.big -> Russian_v1.0.big
                    string fileNameWithVersion = $"{fileNameWithoutExtension}_v{versionNumber}{fileExtension}";
                    string filePath = Path.Combine(versionDownloadDirectory, fileNameWithVersion);

                    await DownloadFileAsync(urlToDownload, filePath);
                    Console.WriteLine($"Downloaded language file for '{language}' (v{versionNumber}): {filePath}");

                    // Добавляем имя файла (только имя, не полный путь) в список для skudef
                    // skudef будет в versionDownloadDirectory, языковой файл тоже, поэтому просто имя.
                    listOfBigFilesForSkudef.Add(fileNameWithVersion);
                }
            }
            else
            {
                Console.WriteLine($"Language file for '{language}' not found in version {versionNumber} of mod {mod.Name}.");
            }

            // 4. Создаём skudef файл
            if (listOfBigFilesForSkudef.Count != 0)
            {
                // Имя skudef файла теперь отражает, что ExampleMod и версия находятся на одном уровне
                string skudefFileName = $"{mod.Name}-{language}_v{versionNumber}.skudef";
                string skudefFilePath = Path.Combine(versionDownloadDirectory, skudefFileName);

                await CreateSkudefFileAsync(skudefFilePath, requestedModGame, listOfBigFilesForSkudef);
                Console.WriteLine($"Created skudef file: {skudefFilePath}");
                return skudefFilePath;
            }

            Console.WriteLine("No .big or .lyi files were processed, so no skudef file was created.");
            return null;
        }

        /// <summary>
        /// Асинхронно создаёт skudef файл с указанным содержимым.
        /// </summary>
        /// <param name="skudefFilePath">Путь к создаваемому файлу.</param>
        /// <param name="requestedModGame">Значение для строки 'mod'.</param>
        /// <param name="bigFileList">Список относительных путей к .big/.lyi файлам для добавления.</param>
        private static async Task CreateSkudefFileAsync(string skudefFilePath, string requestedModGame, List<string> bigFileList)
        {
            await using StreamWriter writer = new(skudefFilePath, append: false); // Создаём новый файл или перезаписываем
            await writer.WriteLineAsync($"mod-game {requestedModGame}");
            foreach (string relativePath in bigFileList)
            {
                await writer.WriteLineAsync($"add-big {relativePath}");
            }
        }

        // --- Остальные методы остаются без изменений ---
        private static async Task DownloadFileAsync(string url, string filePath)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            await using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);
        }

        private static async Task<bool> IsFileUpToDateAsync(string url, string localFilePath)
        {
            using HttpResponseMessage headResponse = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            headResponse.EnsureSuccessStatusCode();

            long? remoteFileSize = headResponse.Content.Headers.ContentLength;
            if (!remoteFileSize.HasValue)
            {
                Console.WriteLine($"Could not get file size for {url}. Assuming update is needed.");
                return false;
            }

            FileInfo localFileInfo = new(localFilePath);
            if (localFileInfo.Length != remoteFileSize.Value)
            {
                Console.WriteLine($"File size mismatch for {localFilePath}. Update needed.");
                return false;
            }

            Console.WriteLine($"File size matches for {localFilePath}. Assuming it's up-to-date.");
            return true;
        }
    }
}
