// Managers.Github/GitHubModManager.Helpers.cs
using Managers.GithubModManager;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Managers.Github
{
    public static partial class GitHubModManager
    {
        /// <summary>
        /// Асинхронно получает содержимое файла по URL.
        /// </summary>
        /// <param name="downloadUrl">URL файла.</param>
        /// <returns>Содержимое файла в виде строки.</returns>
        private static async Task<string> GetFileContentAsync(string? downloadUrl)
        {
            return string.IsNullOrWhiteSpace(downloadUrl) ? string.Empty : await _httpClient.GetStringAsync(downloadUrl);
        }

        /// <summary>
        /// Асинхронно получает контрольную сумму (SHA) файла из GitHub API.
        /// </summary>
        /// <param name="downloadUrl">URL файла (обычно raw).</param>
        /// <returns>Контрольная сумма или пустая строка.</returns>
        private static async Task<string> GetFileChecksumAsync(string? downloadUrl)
        {
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                return string.Empty;
            }

            string pathToFile = ExtractPathFromRawUrl(downloadUrl);
            if (string.IsNullOrWhiteSpace(pathToFile))
            {
                return string.Empty;
            }

            string apiUrl = $"{GitHubConstants.GitHubApiBaseUrl}/{GitHubConstants.RepositoryOwner}/{GitHubConstants.RepositoryName}/contents/{pathToFile}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();
                    JObject fileInfo = Newtonsoft.Json.Linq.JObject.Parse(jsonContent);

                    string sha = fileInfo[GitHubConstants.ShaParam]?.ToString() ?? string.Empty;
                    return sha;
                }
                else
                {
                    Debug.WriteLine($"Ошибка получения SHA для {downloadUrl}: {response.StatusCode}");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Исключение при получении SHA для {downloadUrl}: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Извлекает путь к файлу из raw URL GitHub.
        /// </summary>
        /// <param name="rawUrl">Raw URL файла.</param>
        /// <returns>Путь к файлу внутри репозитория.</returns>
        private static string ExtractPathFromRawUrl(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return string.Empty;
            }

            Uri uri = new(rawUrl);
            string path = uri.AbsolutePath;

            string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            return parts.Length >= 3 ? string.Join("/", parts.Skip(3)) : string.Empty;
        }
    }
}