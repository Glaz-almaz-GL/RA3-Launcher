// Managers.Github/GitHubModManager.VersionParsing.cs
using Items.Mod;
using Newtonsoft.Json.Linq;
using RA3_Launcher.Managers.GithubModManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Managers.Github
{
    public static partial class GitHubModManager
    {
        /// <summary>
        /// Асинхронно парсит информацию о версии мода.
        /// </summary>
        /// <param name="fullVersionPath">Полный путь к каталогу версии.</param>
        /// <param name="versionName">Имя версии.</param>
        /// <param name="commonMainModFilesUrls">Список URL-адресов общих файлов.</param>
        /// <returns>Объект <see cref="ModVersion"/>.</returns>
        private static async Task<ModVersion> ParseVersionAsync(string fullVersionPath, string versionName, List<string> commonMainModFilesUrls)
        {
            ModVersion version = new() { VersionNumber = versionName };

            JArray versionContents = await GetRepositoryContentsAsync(fullVersionPath);

            JToken? versionInfoFile = FindFileInContents(versionContents, GitHubConstants.VersionInfoFileName);
            if (versionInfoFile != null)
            {
                string versionInfoContent = await GetFileContentAsync(versionInfoFile[GitHubConstants.DownloadUrlParam]?.ToString() ?? string.Empty);
                ParseVersionInfoFile(version, versionInfoContent);
            }

            // Обработка всех файлов в версии
            await ProcessAllFilesInVersion(versionContents, version, fullVersionPath);

            // Добавление общих файлов мода
            foreach (string commonFileUrl in commonMainModFilesUrls)
            {
                string fileName = Path.GetFileName(new Uri(commonFileUrl).LocalPath);
                ModFileInfo commonFileInfo = new()
                {
                    FileName = fileName,
                    DownloadUrl = commonFileUrl, // Может быть обновлён позже, если это LFS
                    IsCommonFile = true,
                    FileType = fileName.EndsWith(".lyi", StringComparison.OrdinalIgnoreCase) ? ModFileType.Lyi : ModFileType.Big
                };

                // Обработка LFS для общего файла
                await ProcessLfsAsync(commonFileInfo);

                version.AllFiles[commonFileInfo.FileName] = commonFileInfo;
            }

            return version;
        }

        /// <summary>
        /// Асинхронно обрабатывает все файлы и подкаталоги в каталоге версии.
        /// </summary>
        /// <param name="versionContents">Содержимое каталога версии.</param>
        /// <param name="version">Объект версии для обновления.</param>
        /// <param name="fullVersionPath">Полный путь к каталогу версии.</param>
        private static async Task ProcessAllFilesInVersion(JArray versionContents, ModVersion version, string fullVersionPath)
        {
            foreach (JToken item in versionContents)
            {
                string? itemType = item[GitHubConstants.TypeParam]?.ToString();
                string? itemName = item[GitHubConstants.NameParam]?.ToString();

                if (itemType == "file" && !string.IsNullOrEmpty(itemName))
                {
                    // Обработка обычного файла
                    await ProcessFile(item, version);
                }
                else if (itemType == "dir" && !string.IsNullOrEmpty(itemName) && itemName.Equals(GitHubConstants.LanguagesFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    string languagesPath = $"{fullVersionPath}/{GitHubConstants.LanguagesFolderName}";
                    JArray languagesContents = await GetRepositoryContentsAsync(languagesPath);
                    await ProcessLanguageFiles(languagesContents, version);
                }
            }
        }

        /// <summary>
        /// Асинхронно обрабатывает обычный файл в версии.
        /// </summary>
        /// <param name="fileItem">Токен файла.</param>
        /// <param name="version">Объект версии для обновления.</param>
        private static async Task ProcessFile(JToken fileItem, ModVersion version)
        {
            string? fileName = fileItem[GitHubConstants.NameParam]?.ToString();
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            string? downloadUrl = fileItem[GitHubConstants.DownloadUrlParam]?.ToString();
            if (string.IsNullOrEmpty(downloadUrl))
            {
                return;
            }

            int size = fileItem[GitHubConstants.SizeParam]?.ToObject<int>() ?? 0;
            string sha = await GetFileChecksumAsync(downloadUrl);
            ModFileType fileType = GetFileType(fileName);

            ModFileInfo fileInfo = new()
            {
                FileName = fileName,
                DownloadUrl = downloadUrl, // Может быть обновлён позже, если это LFS
                Size = size,               // Может быть обновлён позже, если это LFS
                Checksum = sha,            // Может быть обновлён позже, если это LFS
                FileType = fileType,
                LastModified = version.UpdateDate
            };

            // Проверяем, является ли файл основным файлом версии (находится в каталоге версии, не в Languages)
            // и имеет расширение .big или .lyi
            if (fileName.EndsWith(".big", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".lyi", StringComparison.OrdinalIgnoreCase))
            {
                fileInfo.IsVersionMainFile = true; // <-- Помечаем как основной файл версии
                version.VersionMainFiles.Add(fileInfo); // <-- Добавляем в список основных файлов версии
            }

            // Проверка и обработка LFS
            await ProcessLfsAsync(fileInfo);

            version.AllFiles[fileInfo.FileName] = fileInfo;
        }

        /// <summary>
        /// Асинхронно обрабатывает файлы в подкаталоге Languages.
        /// </summary>
        /// <param name="languagesContents">Содержимое каталога Languages.</param>
        /// <param name="version">Объект версии для обновления.</param>
        private static async Task ProcessLanguageFiles(JArray languagesContents, ModVersion version)
        {
            foreach (JToken langFile in languagesContents)
            {
                await ProcessLanguageFile(langFile, version);
            }
        }

        /// <summary>
        /// Асинхронно обрабатывает файл локализации.
        /// </summary>
        /// <param name="langFile">Токен файла локализации.</param>
        /// <param name="version">Объект версии для обновления.</param>
        private static async Task ProcessLanguageFile(JToken langFile, ModVersion version)
        {
            string? fileName = langFile[GitHubConstants.NameParam]?.ToString();
            if (string.IsNullOrEmpty(fileName) || (!fileName.EndsWith(".big", StringComparison.OrdinalIgnoreCase) && !fileName.EndsWith(".lyi", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            string? languageName = ExtractLanguageNameFromFileName(fileName);
            if (string.IsNullOrEmpty(languageName))
            {
                return;
            }

            string? downloadUrl = langFile[GitHubConstants.DownloadUrlParam]?.ToString();
            if (string.IsNullOrEmpty(downloadUrl))
            {
                return;
            }

            int size = langFile[GitHubConstants.SizeParam]?.ToObject<int>() ?? 0;
            string sha = await GetFileChecksumAsync(downloadUrl);
            ModFileType fileType = GetFileType(fileName);

            ModFileInfo langFileInfo = new()
            {
                FileName = fileName,
                DownloadUrl = downloadUrl, // Может быть обновлён позже, если это LFS
                Size = size,               // Может быть обновлён позже, если это LFS
                Checksum = sha,            // Может быть обновлён позже, если это LFS
                LanguageCode = languageName,
                FileType = fileType,
                LastModified = version.UpdateDate
            };

            // Проверка и обработка LFS для языкового файла
            await ProcessLfsAsync(langFileInfo);

            // Файлы из Languages теперь помечаются как Common (по новому определению)
            langFileInfo.IsCommonFile = true; // <-- Помечаем как Common файл (локализация)

            version.AllFiles[langFileInfo.FileName] = langFileInfo;

            if (!version.AvailableLanguages.Contains(languageName))
            {
                version.AvailableLanguages.Add(languageName);
            }
        }

        /// <summary>
        /// Определяет тип файла по расширению.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <returns><see cref="ModFileType"/>.</returns>
        private static ModFileType GetFileType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".lyi" => ModFileType.Lyi,
                ".big" => ModFileType.Big,
                ".str" => ModFileType.Str,
                ".txt" => ModFileType.Txt,
                ".skudef" => ModFileType.Skudef,
                _ => ModFileType.Other
            };
        }
    }
}