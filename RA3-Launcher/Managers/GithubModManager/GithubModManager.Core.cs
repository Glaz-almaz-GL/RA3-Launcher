using DynamicData;
using Items.Mod;
using Managers.AvaloniaManagers;
using Managers.GithubModManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Managers.Github
{
    /// <summary>
    /// Статический класс для управления модами через GitHub API.
    /// </summary>
    public static partial class GitHubModManager
    {
        private static readonly HttpClient _httpClient = new(new HttpClientHandler()
        {
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
        });

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
            if (string.IsNullOrWhiteSpace(token))
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
        /// <returns>Список объектов <see cref="ModMetadata"/>.</returns>
        public static async Task<List<ModMetadata>> GetModsAsync()
        {
            List<ModMetadata> mods = [];
            JArray repositoryContents;

            try
            {
                repositoryContents = await GetRepositoryContentsAsync("");
            }
            catch
            {
                return mods;
            }

            List<string> modDirs = [.. repositoryContents
                .Where(item => item[GitHubConstants.TypeParam]?.ToString() == "dir" &&
                              !GitHubConstants.ModsFolderName.Equals(item[GitHubConstants.NameParam]?.ToString(), StringComparison.OrdinalIgnoreCase))
                .Select(item => item[GitHubConstants.NameParam]?.ToString()!)];

            const int maxConcurrency = 4;
            using SemaphoreSlim semaphore = new(maxConcurrency, maxConcurrency);
            List<Task<ModMetadata?>> results = [];

            foreach (string modName in modDirs)
            {
                results.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        return await ParseModAsync(modName);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            try
            {
                ModMetadata?[] parsedMods = await Task.WhenAll(results);
                mods.AddRange(parsedMods.Where(m => m != null)!);
            }
            catch (Exception ex)
            {
                GrowlsManager.ShowErrorMsg(ex, "Ошибка при обработке модификаций", false);
            }

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
        /// <returns>Объект <see cref="ModMetadata"/> или null, если возникла ошибка.</returns>
        private static async Task<ModMetadata?> ParseModAsync(string modName)
        {
            try
            {
                JArray modContents = await GetRepositoryContentsAsync(modName);
                ModMetadata? mod = await InitializeModMetadata(modName, modContents);

                if (mod == null)
                {
                    return null;
                }

                // Parse common (root-level) .big/.lyi files
                await ParseCommonModFilesAsync(mod, modContents);

                // Parse versions (either in /Versions/ or root subdirs)
                List<ModVersionMetadata> versions = await ParseVersionsAsync(modName, modContents);
                mod.Versions = versions;

                // Set latest version and last updated
                if (versions.Count > 0)
                {
                    ModVersionMetadata? latest = versions.MaxBy(v => v.ReleaseDate);
                    if (latest != null)
                    {
                        mod.LatestVersion = latest.Version;
                        mod.LastUpdated = latest.ReleaseDate;
                    }
                }

                return mod;
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("The SSL connection could not be established"))
                {
                    GrowlsManager.ShowErrorMsg("Не удалось установить SSL-соединение.");
                    Debug.WriteLine("Не удалось установить SSL-соединение.");
                }
                else
                {
                    throw;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при парсинге мода {modName}: {ex}");
                return null;
            }
        }

        private static async Task<ModMetadata?> InitializeModMetadata(string modName, JArray modContents)
        {
            JToken? infoFile = FindFileInContents(modContents, GitHubConstants.InfoFileName);
            if (infoFile == null)
            {
                return null; // Optional: still allow mod without ModInfo.txt?
            }

            string content = await GetFileContentAsync(infoFile[GitHubConstants.DownloadUrlParam]?.ToString() ?? "");
            ModMetadata mod = new()
            {
                Name = modName,
                MainFiles = [],
            };
            await ParseInfoFileAsync(mod, content);
            return mod;
        }

        private static async Task ParseCommonModFilesAsync(ModMetadata mod, JArray modContents)
        {
            foreach (JToken fileItem in modContents)
            {
                if (fileItem[GitHubConstants.TypeParam]?.ToString() != "file")
                {
                    continue;
                }

                string? fileName = fileItem[GitHubConstants.NameParam]?.ToString();
                string? downloadUrl = fileItem[GitHubConstants.DownloadUrlParam]?.ToString();

                if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(downloadUrl))
                {
                    continue;
                }

                if (!IsGameFile(fileName))
                {
                    continue;
                }

                int size = fileItem[GitHubConstants.SizeParam]?.ToObject<int>() ?? 0;
                string? sha = await GetFileChecksumAsync(downloadUrl);

                ModFileInfo fileInfo = new()
                {
                    FileName = fileName,
                    DownloadUrl = downloadUrl,
                    Size = size,
                    Checksum = sha,
                    FileType = IsLyiFile(fileName) ? ModFileType.Lyi : ModFileType.Big,
                    IsCommonFile = true,
                    IsModMainFile = true,
                    LastModified = mod.LastUpdated // or DateTime.Now if not yet set
                };

                await ProcessLfsAsync(fileInfo);
                mod.MainFiles.Add(fileInfo);
            }
        }

        private static bool IsGameFile(string fileName)
        {
            return fileName.EndsWith(".big", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".lyi", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLyiFile(string fileName)
        {
            return fileName.EndsWith(".lyi", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<List<ModVersionMetadata>> ParseVersionsAsync(string modName, JArray modContents)
        {
            JToken? versionsDir = FindFileInContents(modContents, GitHubConstants.VersionsFolderName);

            return versionsDir != null && IsDirectory(versionsDir)
                ? await ParseVersionsFromSubfolderAsync(modName)
                : await ParseVersionsFromRootAsync(modName, modContents);
        }

        // Вспомогательный метод: проверяет, является ли элемент каталогом
        private static bool IsDirectory(JToken item)
        {
            return item[GitHubConstants.TypeParam]?.ToString() == "dir";
        }

        // Обработка версий из подпапки /Versions/
        private static async Task<List<ModVersionMetadata>> ParseVersionsFromSubfolderAsync(string modName)
        {
            List<ModVersionMetadata> versions = [];
            string versionsPath = $"{modName}/{GitHubConstants.VersionsFolderName}";
            JArray versionsContents = await GetRepositoryContentsAsync(versionsPath);

            foreach (JToken vDir in versionsContents)
            {
                if (!IsDirectory(vDir))
                {
                    continue;
                }

                string? versionName = vDir[GitHubConstants.NameParam]?.ToString();
                if (string.IsNullOrWhiteSpace(versionName))
                {
                    continue;
                }

                ModVersionMetadata? versionMetadata = await ParseVersionAsync(
                    $"{modName}/{GitHubConstants.VersionsFolderName}/{versionName}",
                    versionName,
                    []
                );

                if (versionMetadata != null)
                {
                    versions.Add(versionMetadata);
                }
            }

            return versions;
        }

        // Обработка версий, лежащих напрямую в корне мода
        private static async Task<List<ModVersionMetadata>> ParseVersionsFromRootAsync(string modName, JArray modContents)
        {
            List<ModVersionMetadata> versions = [];

            IEnumerable<string> versionNames = modContents
                .Where(IsDirectory)
                .Select(v => v[GitHubConstants.NameParam]?.ToString())
                .Where(name => !string.IsNullOrWhiteSpace(name) && !IsNonVersionDirectory(name))
                .Cast<string>();

            foreach (string versionName in versionNames)
            {
                ModVersionMetadata? versionMetadata = await ParseVersionAsync(
                    $"{modName}/{versionName}",
                    versionName,
                    []
                );

                if (versionMetadata != null)
                {
                    versions.Add(versionMetadata);
                }
            }

            return versions;
        }

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