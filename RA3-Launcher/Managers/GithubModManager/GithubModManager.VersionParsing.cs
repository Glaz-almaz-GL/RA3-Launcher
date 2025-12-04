// Managers.Github/GitHubModManager.VersionParsing.cs
using Items.Mod;
using Managers.AvaloniaManagers;
using Managers.GithubModManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Managers.Github
{
    public static partial class GitHubModManager
    {
        /// <summary>
        /// Асинхронно парсит одну версию мода и возвращает метаданные версии.
        /// </summary>
        /// <param name="fullVersionPath">Полный путь к каталогу версии (например, "ModName/Versions/1.2").</param>
        /// <param name="versionName">Имя версии (например, "1.2").</param>
        /// <param name="commonMainModFilesUrls">Список URL общих файлов из корня мода (.big/.lyi).</param>
        /// <returns>Объект <see cref="ModVersionMetadata"/> или null в случае ошибки.</returns>
        private static async Task<ModVersionMetadata?> ParseVersionAsync(
            string fullVersionPath,
            string versionName,
            List<string> commonMainModFilesUrls)
        {
            try
            {
                ModVersionMetadata version = new()
                {
                    Version = versionName,
                    Files = [],
                    SupportedLanguages = []
                };

                JArray versionContents = await GetRepositoryContentsAsync(fullVersionPath);

                // Parse VersionInfo.txt if present
                await ParseAndApplyVersionInfoAsync(version, versionContents);

                // Process top-level files and the Languages/ folder
                await ProcessVersionContentsAsync(version, fullVersionPath, versionContents);

                // Add common mod files from root (e.g., Mod-Main.lyi)
                await AddCommonModFilesAsync(version, commonMainModFilesUrls);

                return version;
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("The SSL connection could not be established"))
                {
                    GrowlsManager.ShowErrorMsg("Не удалось установить SSL-соединение. Попробуй ещё раз чуть позже.", "Ошибка при получении информации о модификации.");
                    return null;
                }
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при парсинге версии {versionName}: {ex}");
                return null;
            }
        }

        private static async Task ParseAndApplyVersionInfoAsync(ModVersionMetadata version, JArray versionContents)
        {
            JToken? versionInfoFile = FindFileInContents(versionContents, GitHubConstants.VersionInfoFileName);
            if (versionInfoFile != null)
            {
                string url = versionInfoFile[GitHubConstants.DownloadUrlParam]?.ToString() ?? "";
                string content = await GetFileContentAsync(url);
                ParseVersionInfoFile(version, content);
            }
        }

        private static async Task ProcessVersionContentsAsync(
            ModVersionMetadata version,
            string fullVersionPath,
            JArray versionContents)
        {
            foreach (JToken item in versionContents)
            {
                string? itemType = item[GitHubConstants.TypeParam]?.ToString();
                string? itemName = item[GitHubConstants.NameParam]?.ToString();

                if (itemType == "file" && !string.IsNullOrWhiteSpace(itemName))
                {
                    await ProcessTopLevelFileAsync(version, item);
                }
                else if (itemType == "dir" &&
                         !string.IsNullOrWhiteSpace(itemName) &&
                         itemName.Equals(GitHubConstants.LanguagesFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    string langPath = $"{fullVersionPath}/{GitHubConstants.LanguagesFolderName}";
                    await ProcessLanguageFolderAsync(version, langPath);
                }
            }
        }

        private static async Task ProcessTopLevelFileAsync(ModVersionMetadata version, JToken fileItem)
        {
            string? downloadUrl = fileItem[GitHubConstants.DownloadUrlParam]?.ToString();
            int size = fileItem[GitHubConstants.SizeParam]?.ToObject<int>() ?? 0;
            string? fileName = fileItem[GitHubConstants.NameParam]?.ToString();

            if (string.IsNullOrWhiteSpace(downloadUrl) || string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            ModFileType fileType = GetFileType(fileName);
            if (fileType is not (ModFileType.Big or ModFileType.Lyi))
            {
                return;
            }

            ModFileInfo fileInfo = new()
            {
                FileName = fileName,
                DownloadUrl = downloadUrl,
                Size = size,
                Checksum = await GetFileChecksumAsync(downloadUrl),
                FileType = fileType,
                IsVersionMainFile = true,
                LastModified = version.ReleaseDate
            };

            await ProcessLfsAsync(fileInfo);
            version.Files.Add(fileInfo);
        }

        private static async Task ProcessLanguageFolderAsync(ModVersionMetadata version, string langPath)
        {
            JArray langContents = await GetRepositoryContentsAsync(langPath);

            foreach (JToken langFile in langContents)
            {
                if (langFile[GitHubConstants.TypeParam]?.ToString() != "file")
                {
                    continue;
                }

                string? langFileName = langFile[GitHubConstants.NameParam]?.ToString();
                string? langDownloadUrl = langFile[GitHubConstants.DownloadUrlParam]?.ToString();
                int langSize = langFile[GitHubConstants.SizeParam]?.ToObject<int>() ?? 0;

                if (string.IsNullOrWhiteSpace(langFileName) || string.IsNullOrWhiteSpace(langDownloadUrl))
                {
                    continue;
                }

                ModFileType langFileType = GetFileType(langFileName);
                if (langFileType is not (ModFileType.Big or ModFileType.Lyi))
                {
                    continue;
                }

                string? languageCode = Path.GetFileNameWithoutExtension(langFileName);
                if (string.IsNullOrWhiteSpace(languageCode))
                {
                    continue;
                }

                ModFileInfo langFileInfo = new()
                {
                    FileName = langFileName,
                    DownloadUrl = langDownloadUrl,
                    Size = langSize,
                    Checksum = await GetFileChecksumAsync(langDownloadUrl),
                    FileType = langFileType,
                    LanguageCode = languageCode,
                    IsCommonFile = true,
                    LastModified = version.ReleaseDate
                };

                await ProcessLfsAsync(langFileInfo);
                version.Files.Add(langFileInfo);
                version.SupportedLanguages.Add(languageCode);
            }
        }

        private static async Task AddCommonModFilesAsync(ModVersionMetadata version, List<string> commonUrls)
        {
            foreach (string commonUrl in commonUrls)
            {
                string fileName = Path.GetFileName(new Uri(commonUrl).LocalPath);
                ModFileType fileType = GetFileType(fileName);

                if (fileType is not (ModFileType.Big or ModFileType.Lyi))
                {
                    continue;
                }

                ModFileInfo commonFileInfo = new()
                {
                    FileName = fileName,
                    DownloadUrl = commonUrl,
                    Size = 0, // Size not available here; could be improved by passing it in future
                    Checksum = await GetFileChecksumAsync(commonUrl),
                    FileType = fileType,
                    IsCommonFile = true,
                    IsModMainFile = true,
                    LastModified = version.ReleaseDate
                };

                await ProcessLfsAsync(commonFileInfo);
                version.Files.Add(commonFileInfo);
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