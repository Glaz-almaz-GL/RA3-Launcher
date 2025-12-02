// Managers.Github/GitHubModManager.Core.cs
using Items.Mod;
using Newtonsoft.Json.Linq;
using RA3_Launcher.Managers.GithubModManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Managers.Github
{
    /// <summary>
    /// Статический класс для управления модами через GitHub API.
    /// </summary>
    public static partial class GitHubModManager
    {
        private static readonly HttpClient _httpClient = new();

        /// <summary>
        /// Инициализирует HTTP-клиент с необходимыми заголовками для работы с GitHub API.
        /// </summary>
        public static void Initialize()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ModManager");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GitHubSettings.GithubToken);
            _httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

            string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            Debug.WriteLine($"GITHUB_TOKEN из окружения: '{token}'");
            if (string.IsNullOrEmpty(token))
            {
                Debug.WriteLine("Предупреждение: Переменная GITHUB_TOKEN не найдена или пуста.");
            }
            else
            {
                Debug.WriteLine("Токен успешно получен из переменной окружения.");
            }
        }

        /// <summary>
        /// Асинхронно получает список модов из репозитория GitHub.
        /// </summary>
        /// <returns>Список объектов <see cref="Mod"/>.</returns>
        public static async Task<ICollection<Mod>> GetModsAsync()
        {
            var mods = new List<Mod>();

            JArray repositoryContents;
            try
            {
                repositoryContents = await GetRepositoryContentsAsync("");
            }
            catch
            {
                // Ошибка уже отображена в GrowlsManager внутри GetRepositoryContentsAsync
                return mods; // возвращаем пустой список
            }

            // Собираем задачи параллельно (но не слишком агрессивно)
            var parseTasks = new List<Task<Mod?>>();

            foreach (JToken item in repositoryContents)
            {
                string? itemType = item[GitHubConstants.TypeParam]?.ToString();
                string? itemName = item[GitHubConstants.NameParam]?.ToString();

                if (itemType == "dir" &&
                    !string.IsNullOrEmpty(itemName) &&
                    !itemName.Equals(GitHubConstants.ModsFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    // Запускаем парсинг параллельно
                    parseTasks.Add(ParseModAsync(itemName));
                }
            }

            try
            {
                // Дожидаемся всех параллельных задач
                var parsedMods = await Task.WhenAll(parseTasks);

                // Фильтруем null и добавляем в список
                foreach (var mod in parsedMods)
                {
                    if (mod is not null)
                        mods.Add(mod);
                }
            }
            catch (Exception ex)
            {
                // Общая ошибка при парсинге (редко, но возможно)
                GrowlsManager.ShowErrorMsg(ex, "Ошибка при обработке модификаций", false);
            }

            Debug.WriteLine($"Получено {mods.Count} модов.");

            return mods;
        }

        /// <summary>
        /// Асинхронно получает содержимое каталога из репозитория GitHub.
        /// </summary>
        /// <param name="path">Путь к каталогу внутри <see cref="GitHubConstants.ModsFolderName"/>.</param>
        /// <returns><see cref="JArray"/> с информацией о содержимом.</returns>
        private static async Task<JArray> GetRepositoryContentsAsync(string path)
        {
            string requestUrl = $"{GitHubConstants.GitHubApiBaseUrl}/{GitHubConstants.RepositoryOwner}/{GitHubConstants.RepositoryName}/contents/{GitHubConstants.ModsFolderName}/{path}";

            Debug.WriteLine($"Request Url: {requestUrl}");

            try
            {
                string response = await _httpClient.GetStringAsync(requestUrl);

                return JArray.Parse(response);
            }
            catch (Exception ex)
            {
                GrowlsManager.ShowErrorMsg(ex, "Ошибка при получении списка модификаций", false);
                return [];
            }
        }

        /// <summary>
        /// Асинхронно парсит информацию о моде по его имени.
        /// </summary>
        /// <param name="modName">Имя каталога мода.</param>
        /// <returns>Объект <see cref="Mod"/> или null, если возникла ошибка.</returns>
        private static async Task<Mod?> ParseModAsync(string modName)
        {
            try
            {
                Mod mod = new() { Name = modName };

                JArray modContents = await GetRepositoryContentsAsync(modName);

                await ProcessModInfo(mod, modContents);
                List<string> commonMainModFilesUrls = FindCommonMainModFiles(modContents);

                // Добавляем файлы из корня мода (Main Files of the Mod) в MainFiles мода
                foreach (string commonFileUrl in commonMainModFilesUrls)
                {
                    string fileName = Path.GetFileName(new Uri(commonFileUrl).LocalPath);
                    ModFileInfo modMainFileInfo = new()
                    {
                        FileName = fileName,
                        DownloadUrl = commonFileUrl,
                        IsModMainFile = true, // <-- Помечаем как основной файл мода
                        FileType = fileName.EndsWith(".lyi", StringComparison.OrdinalIgnoreCase) ? ModFileType.Lyi : ModFileType.Big
                    };

                    // Обработка LFS для файла из корня мода
                    await ProcessLfsAsync(modMainFileInfo);

                    mod.MainFiles.Add(modMainFileInfo); // <-- Добавляем в MainFiles мода
                }

                await ProcessModVersions(mod, modName, modContents, commonMainModFilesUrls);

                SetModLatestVersion(mod);

                return mod;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при обработке мода {modName}: {ex.Message} {ex.InnerException}");
                return null;
            }
        }

        /// <summary>
        /// Находит URL-адреса общих файлов мода (.big, .lyi) на верхнем уровне каталога мода.
        /// </summary>
        /// <param name="modContents">Содержимое каталога мода.</param>
        /// <returns>Список URL-адресов файлов.</returns>
        private static List<string> FindCommonMainModFiles(JArray modContents)
        {
            List<string> commonMainModFiles = [.. modContents.Where(c =>
            {
                string? name = c[GitHubConstants.NameParam]?.ToString();
                return name != null && (name.EndsWith(".big", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".lyi", StringComparison.OrdinalIgnoreCase));
            }).Select(c => c[GitHubConstants.DownloadUrlParam]?.ToString()).Where(url => !string.IsNullOrEmpty(url)).Cast<string>()];

            return commonMainModFiles;
        }

        /// <summary>
        /// Асинхронно обрабатывает файл ModInfo.txt для заполнения базовой информации о моде.
        /// </summary>
        /// <param name="mod">Объект мода для обновления.</param>
        /// <param name="modContents">Содержимое каталога мода.</param>
        private static async Task ProcessModInfo(Mod mod, JArray modContents)
        {
            JToken? infoFile = FindFileInContents(modContents, GitHubConstants.InfoFileName);
            if (infoFile != null)
            {
                string infoContent = await GetFileContentAsync(infoFile?[GitHubConstants.DownloadUrlParam]?.ToString() ?? string.Empty);
                ParseInfoFile(mod, infoContent);
            }
        }

        /// <summary>
        /// Асинхронно обрабатывает версии мода.
        /// </summary>
        /// <param name="mod">Объект мода для обновления.</param>
        /// <param name="modPath">Путь к каталогу мода.</param>
        /// <param name="modContents">Содержимое каталога мода.</param>
        /// <param name="commonMainModFilesUrls">Список URL-адресов общих файлов.</param>
        private static async Task ProcessModVersions(Mod mod, string modPath, JArray modContents, List<string> commonMainModFilesUrls)
        {
            JToken? versionsDir = FindFileInContents(modContents, GitHubConstants.VersionsFolderName);

            if (versionsDir?[GitHubConstants.TypeParam]?.ToString() == "dir")
            {
                string versionsPath = $"{modPath}/{GitHubConstants.VersionsFolderName}";
                JArray versionsContents = await GetRepositoryContentsAsync(versionsPath);

                List<JToken> versionDirectories = [.. versionsContents.Where(c => c[GitHubConstants.TypeParam]?.ToString() == "dir")];

                await ProcessModDirs(mod, versionDirectories, modPath, commonMainModFilesUrls);
            }
            else
            {
                List<JToken> versionDirectories = [.. modContents.Where(c => c[GitHubConstants.TypeParam]?.ToString() == "dir")];

                await ProcessModDirs(mod, versionDirectories, modPath, commonMainModFilesUrls);
            }
        }

        /// <summary>
        /// Асинхронно обрабатывает каталоги версий мода.
        /// </summary>
        /// <param name="mod">Объект мода для обновления.</param>
        /// <param name="versionDirectories">Список токенов каталогов версий.</param>
        /// <param name="modPath">Путь к каталогу мода.</param>
        /// <param name="commonMainModFilesUrls">Список URL-адресов общих файлов.</param>
        private static async Task ProcessModDirs(Mod mod, List<JToken> versionDirectories, string modPath, List<string> commonMainModFilesUrls)
        {
            foreach (JToken versionDir in versionDirectories)
            {
                string? versionName = versionDir[GitHubConstants.NameParam]?.ToString();
                if (string.IsNullOrEmpty(versionName) || IsNonVersionDirectory(versionName))
                {
                    continue; // Пропускаем, если имя пустое или это системный каталог
                }

                await ProcessSingleVersionAsync(mod, modPath, versionName, commonMainModFilesUrls);
            }
        }

        /// <summary>
        /// Асинхронно обрабатывает одну версию мода.
        /// </summary>
        /// <param name="mod">Объект мода для обновления.</param>
        /// <param name="modPath">Путь к каталогу мода.</param>
        /// <param name="versionName">Имя версии.</param>
        /// <param name="commonMainModFilesUrls">Список URL-адресов общих файлов.</param>
        private static async Task ProcessSingleVersionAsync(Mod mod, string modPath, string versionName, List<string> commonMainModFilesUrls)
        {
            string fullVersionPath = $"{modPath}/{GitHubConstants.VersionsFolderName}/{versionName}";
            ModVersion version = await ParseVersionAsync(fullVersionPath, versionName, commonMainModFilesUrls);
            if (version != null)
            {
                mod.Versions.Add(version);
                UpdateModAvailableLanguages(mod, version);
            }
        }
        /// <summary>
        /// Добавляет файлы версии (IsMainFile или IsCommonFile) в список MainFiles мода, избегая дубликатов.
        /// </summary>
        /// <param name="mod">Объект мода для обновления.</param>
        /// <param name="version">Объект версии, файлы которой нужно добавить.</param>
        //private static void AddVersionFilesToModMainFiles(Mod mod, ModVersion version)
        //{
        //    foreach (var file in version.AllFiles.Values)
        //    {
        //        if ((file.IsMainFile || file.IsCommonFile) && !mod.MainFiles.Any(mf => mf.FileName == file.FileName))
        //        {
        //            mod.MainFiles.Add(file);
        //        }
        //    }
        //}

        /// <summary>
        /// Проверяет, является ли имя каталога системным (не версией).
        /// </summary>
        /// <param name="dirName">Имя каталога.</param>
        /// <returns>True, если это системный каталог.</returns>
        private static bool IsNonVersionDirectory(string dirName)
        {
            HashSet<string> nonVersionDirs = new(StringComparer.OrdinalIgnoreCase)
            {
                GitHubConstants.LanguagesFolderName,
                GitHubConstants.VersionsFolderName
            };
            return nonVersionDirs.Contains(dirName);
        }

        /// <summary>
        /// Обновляет список доступных языков для мода на основе версии.
        /// </summary>
        /// <param name="mod">Объект мода для обновления.</param>
        /// <param name="version">Объект версии мода.</param>
        private static void UpdateModAvailableLanguages(Mod mod, ModVersion version)
        {
            foreach (string lang in version.AvailableLanguages)
            {
                if (!mod.AvailableLanguages.Contains(lang))
                {
                    mod.AvailableLanguages.Add(lang);
                }
            }
        }

        /// <summary>
        /// Устанавливает последнюю версию и дату обновления мода.
        /// </summary>
        /// <param name="mod">Объект мода для обновления.</param>
        private static void SetModLatestVersion(Mod mod)
        {
            if (mod.Versions.Count != 0)
            {
                mod.LatestVersion = mod.Versions.MaxBy(v => v.VersionNumber)?.VersionNumber;
                mod.LastUpdated = mod.Versions.Max(v => v.UpdateDate);
            }
        }

        /// <summary>
        /// Находит файл в содержимом каталога по имени.
        /// </summary>
        /// <param name="contents">Содержимое каталога.</param>
        /// <param name="fileName">Имя файла для поиска.</param>
        /// <returns>Токен файла или null, если не найден.</returns>
        private static JToken? FindFileInContents(JArray contents, string fileName)
        {
            return contents.FirstOrDefault(c => c[GitHubConstants.NameParam]?.ToString() == fileName);
        }
    }
}