using Items.Mod;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Managers.Github
{
    public static partial class GitHubModManager
    {
        private static readonly HttpClient _httpClient = new();
        private const string _repositoryOwner = "Glaz-almaz-GL";
        private const string _repositoryName = "RA3-Translations";
        private const string DateTimeRA3Format = "yy-MM-dd:HH-mm";
        private const string DownloadUrlParam = "download_url";
        private const string ModsFolderName = "Mods";
        private const string GitHubApiBaseUrl = "https://api.github.com/repos";

        public static void Initialize()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ModManager");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        }

        public static async Task<List<Mod>> GetModsAsync()
        {
            List<Mod> mods = [];

            var repositoryContents = await GetRepositoryContentsAsync("");

            foreach (var item in repositoryContents)
            {
                var itemType = item["type"]?.ToString();
                var itemName = item["name"]?.ToString();

                if (itemType == "dir" && !string.IsNullOrEmpty(itemName) && !itemName.Equals("Mods", StringComparison.OrdinalIgnoreCase))
                {
                    var mod = await ParseModAsync(itemName);
                    if (mod != null)
                    {
                        mods.Add(mod);
                    }
                }
            }

            return mods;
        }

        private static async Task<JArray> GetRepositoryContentsAsync(string path)
        {
            var requestUrl = $"{GitHubApiBaseUrl}/{_repositoryOwner}/{_repositoryName}/contents/{ModsFolderName}/{path}";

            Debug.WriteLine($"Request Url: {requestUrl}");

            var response = await _httpClient.GetStringAsync(requestUrl);
            return JArray.Parse(response);
        }

        private static async Task<Mod?> ParseModAsync(string modName)
        {
            try
            {
                Mod mod = new() { Name = modName };

                var modContents = await GetRepositoryContentsAsync(modName);

                await ProcessModInfo(mod, modContents);
                var commonMainModFilesUrls = FindCommonMainModFiles(modContents);
                await ProcessModVersions(mod, modName, modContents, commonMainModFilesUrls);

                SetModLatestVersion(mod);

                return mod;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке мода {modName}: {ex.Message}");
                return null;
            }
        }

        private static List<string> FindCommonMainModFiles(JArray modContents)
        {
            var commonMainModFiles = modContents.Where(c =>
            {
                var name = c["name"]?.ToString();
                return name != null && (name.EndsWith(".big", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".lyi", StringComparison.OrdinalIgnoreCase));
            }).Select(c => c[DownloadUrlParam]?.ToString()).Where(url => !string.IsNullOrEmpty(url)).Cast<string>().ToList();

            return commonMainModFiles;
        }

        private static async Task ProcessModInfo(Mod mod, JArray modContents)
        {
            var infoFile = FindFileInContents(modContents, "Info.txt");
            if (infoFile != null)
            {
                var infoContent = await GetFileContentAsync(infoFile?[DownloadUrlParam]?.ToString() ?? string.Empty);
                ParseInfoFile(mod, infoContent);
            }
        }

        private static async Task ProcessModVersions(Mod mod, string modName, JArray modContents, List<string> commonMainModFilesUrls)
        {
            List<JToken> versionDirectories = [.. modContents.Where(c => c["type"]?.ToString() == "dir")];
            foreach (var versionDir in versionDirectories)
            {
                var versionName = versionDir["name"]?.ToString();
                if (!string.IsNullOrEmpty(versionName))
                {
                    var version = await ParseVersionAsync(modName, versionName, commonMainModFilesUrls);
                    if (version != null)
                    {
                        mod.Versions.Add(version);
                        UpdateModAvailableLanguages(mod, version);
                    }
                }
            }
        }

        private static void UpdateModAvailableLanguages(Mod mod, ModVersion version)
        {
            foreach (var lang in version.AvailableLanguages)
            {
                if (!mod.AvailableLanguages.Contains(lang))
                {
                    mod.AvailableLanguages.Add(lang);
                }
            }
        }

        private static void SetModLatestVersion(Mod mod)
        {
            if (mod.Versions.Count != 0)
            {
                mod.LatestVersion = mod.Versions.MaxBy(v => v.VersionNumber)?.VersionNumber;
                mod.LastUpdated = mod.Versions.Max(v => v.UpdateDate);
            }
        }

        private static JToken? FindFileInContents(JArray contents, string fileName)
        {
            return contents.FirstOrDefault(c => c["name"]?.ToString() == fileName);
        }

        private static async Task<ModVersion> ParseVersionAsync(string modName, string versionName, List<string> commonMainModFilesUrls)
        {
            ModVersion version = new() { VersionNumber = versionName };

            var versionPath = $"{modName}/{versionName}";
            var versionContents = await GetRepositoryContentsAsync(versionPath);

            var versionInfoFile = FindFileInContents(versionContents, "VersionInfo.txt");
            if (versionInfoFile != null)
            {
                var versionInfoContent = await GetFileContentAsync(versionInfoFile[DownloadUrlParam]?.ToString() ?? string.Empty);
                ParseVersionInfoFile(version, versionInfoContent);
            }

            var mainModFileInVersion = FindMainModFileInVersion(versionContents);
            if (!string.IsNullOrEmpty(mainModFileInVersion))
            {
                version.MainModFile = mainModFileInVersion;
            }

            version.CommonModFiles = commonMainModFilesUrls;

            var languagesDir = FindFileInContents(versionContents, "Languages");
            if (languagesDir != null)
            {
                var languagesPath = $"{versionPath}/Languages";
                var languagesContents = await GetRepositoryContentsAsync(languagesPath);
                ProcessLanguageFiles(languagesContents, version);
            }

            var skudefDir = FindFileInContents(versionContents, "skudef");
            if (skudefDir != null)
            {
                var skudefPath = $"{versionPath}/skudef";
                var skudefContents = await GetRepositoryContentsAsync(skudefPath);
                ProcessSkudefFiles(skudefContents, version, modName);
            }

            return version;
        }

        private static string? FindMainModFileInVersion(JArray versionContents)
        {
            var mainModFile = versionContents.FirstOrDefault(c =>
            {
                var name = c["name"]?.ToString();
                return name != null && (name.EndsWith(".big", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".lyi", StringComparison.OrdinalIgnoreCase));
            });
            return mainModFile?[DownloadUrlParam]?.ToString();
        }

        private static void ProcessLanguageFiles(JArray languagesContents, ModVersion version)
        {
            foreach (var langFile in languagesContents)
            {
                ProcessSingleLanguageFile(langFile, version);
            }
        }

        // Вспомогательный метод для обработки одного языкового файла
        private static void ProcessSingleLanguageFile(JToken langFile, ModVersion version)
        {
            var fileName = langFile["name"]?.ToString();
            if (string.IsNullOrEmpty(fileName) || (!fileName.EndsWith(".big", StringComparison.OrdinalIgnoreCase) && !fileName.EndsWith(".lyi", StringComparison.OrdinalIgnoreCase)))
            {
                return; // Пропускаем файлы с неподходящим расширением или без имени
            }

            var languageName = ExtractLanguageNameFromFileName(fileName);
            if (string.IsNullOrEmpty(languageName))
            {
                return; // Пропускаем файлы, из которых не удаётся извлечь имя языка
            }

            var downloadUrl = langFile[DownloadUrlParam]?.ToString();
            if (string.IsNullOrEmpty(downloadUrl))
            {
                return; // Пропускаем файлы без URL для скачивания
            }

            // Добавляем файл и язык
            version.LanguageFiles[languageName] = downloadUrl;
            if (!version.AvailableLanguages.Contains(languageName))
            {
                version.AvailableLanguages.Add(languageName);
            }
        }

        private static void ProcessSkudefFiles(JArray skudefContents, ModVersion version, string modName)
        {
            foreach (var skudefFile in skudefContents)
            {
                ProcessSingleSkudefFile(skudefFile, version, modName);
            }
        }

        // Вспомогательный метод для обработки одного skudef файла
        private static void ProcessSingleSkudefFile(JToken skudefFile, ModVersion version, string modName)
        {
            var fileName = skudefFile["name"]?.ToString();
            if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".skudef", StringComparison.OrdinalIgnoreCase))
            {
                return; // Пропускаем файлы с неподходящим расширением или без имени
            }

            var languageName = ExtractLanguageNameFromSkudefFileName(fileName, modName);
            if (string.IsNullOrEmpty(languageName))
            {
                return; // Пропускаем файлы, из которых не удаётся извлечь имя языка
            }

            var downloadUrl = skudefFile[DownloadUrlParam]?.ToString();
            if (string.IsNullOrEmpty(downloadUrl))
            {
                return; // Пропускаем файлы без URL для скачивания
            }

            // Добавляем файл и язык
            if (!version.AvailableLanguages.Contains(languageName))
            {
                version.AvailableLanguages.Add(languageName);
            }
        }

        private static void ParseInfoFile(Mod mod, string content)
        {
            using StringReader reader = new(content);
            string? line;
            string? currentKey = null;
            StringBuilder? currentValueBuilder = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(['='], 2);
                if (parts.Length == 2)
                {
                    ProcessCurrentKeyValue(mod, currentKey, currentValueBuilder);
                    currentKey = parts[0].Trim().ToLower();
                    currentValueBuilder = new StringBuilder(parts[1]);
                }
                else
                {
                    if (currentKey != null && currentValueBuilder != null)
                    {
                        currentValueBuilder.AppendLine(line);
                    }
                }
            }

            ProcessCurrentKeyValue(mod, currentKey, currentValueBuilder);
        }

        private static void ParseVersionInfoFile(ModVersion version, string content)
        {
            using StringReader reader = new(content);
            string? line;
            string? currentKey = null;
            StringBuilder? currentValueBuilder = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(['='], 2);
                if (parts.Length == 2)
                {
                    ProcessCurrentKeyValue(version, currentKey, currentValueBuilder);
                    currentKey = parts[0].Trim().ToLower();
                    currentValueBuilder = new StringBuilder(parts[1]);
                }
                else
                {
                    if (currentKey != null && currentValueBuilder != null)
                    {
                        currentValueBuilder.AppendLine(line);
                    }
                }
            }

            ProcessCurrentKeyValue(version, currentKey, currentValueBuilder);
        }

        private static void ProcessCurrentKeyValue(Mod mod, string? currentKey, StringBuilder? currentValueBuilder)
        {
            if (currentKey != null && currentValueBuilder != null)
            {
                string value = currentValueBuilder.ToString();
                AssignParsedValue(mod, currentKey, value);
            }
        }

        private static void ProcessCurrentKeyValue(ModVersion version, string? currentKey, StringBuilder? currentValueBuilder)
        {
            if (currentKey != null && currentValueBuilder != null)
            {
                string value = currentValueBuilder.ToString();
                AssignParsedValue(version, currentKey, value);
            }
        }

        private static void AssignParsedValue(Mod mod, string key, string value)
        {
            switch (key)
            {
                case "description":
                    mod.Description = value;
                    break;
                case "creation-date":
                    if (DateTime.TryParseExact(value, DateTimeRA3Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var creationDate))
                    {
                        mod.CreationDate = creationDate;
                    }
                    break;
                case "author":
                    mod.Author = value;
                    break;
                case "category":
                case "categories":
                    mod.Category = value;
                    break;
                case "gameversion":
                    mod.GameVersion = value;
                    break;
            }
        }

        private static void AssignParsedValue(ModVersion version, string key, string value)
        {
            switch (key)
            {
                case "changelog":
                    version.Changelog = value;
                    break;
                case "updation-date":
                case "update-date":
                    if (DateTime.TryParseExact(value, DateTimeRA3Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var updateDate))
                    {
                        version.UpdateDate = updateDate;
                    }
                    break;
            }
        }

        private static async Task<string> GetFileContentAsync(string downloadUrl)
        {
            return string.IsNullOrEmpty(downloadUrl) ? string.Empty : await _httpClient.GetStringAsync(downloadUrl);
        }

        private static string? ExtractLanguageNameFromFileName(string fileName)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                return null;
            }
            return fileNameWithoutExtension;
        }

        private static string? ExtractLanguageNameFromSkudefFileName(string fileName, string modName)
        {
            var baseName = fileName.Replace($"{modName}-", "").Replace(".skudef", "");
            return !string.IsNullOrWhiteSpace(baseName) ? baseName : null;
        }
    }
}