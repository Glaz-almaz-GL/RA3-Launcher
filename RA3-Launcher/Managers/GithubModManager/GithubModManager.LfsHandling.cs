// Managers.Github/GitHubModManager.LfsHandling.cs
using Items.Mod;
using RA3_Launcher.Managers.GithubModManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Managers.Github
{
    public static partial class GitHubModManager
    {
        /// <summary>
        /// Асинхронно проверяет, является ли файл указателем Git LFS, и если да, обновляет его метаданные.
        /// </summary>
        /// <param name="fileInfo">Объект <see cref="ModFileInfo"/> для проверки и обновления.</param>
        private static async Task ProcessLfsAsync(ModFileInfo fileInfo)
        {
            // Получаем *содержимое* файла (а не только информацию из API contents)
            string fileContent = await GetFileContentAsync(fileInfo.DownloadUrl);

            if (fileContent.StartsWith(GitHubConstants.LfsVersionLine))
            {
                Debug.WriteLine($"Файл {fileInfo.FileName} является указателем Git LFS. Получаем реальный URL для скачивания...");

                // Извлекаем OID и Size из содержимого указателя
                (string Oid, int Size)? lfsInfo = ParseLfsPointerContent(fileContent);
                if (lfsInfo != null)
                {
                    // Вызываем LFS API для получения реального URL
                    string? lfsDownloadUrl = await GetLfsDownloadUrlAsync(lfsInfo.Value.Oid, lfsInfo.Value.Size);

                    if (!string.IsNullOrEmpty(lfsDownloadUrl))
                    {
                        fileInfo.DownloadUrl = lfsDownloadUrl;
                        fileInfo.Size = lfsInfo.Value.Size; // Используем размер из указателя LFS
                        fileInfo.Checksum = lfsInfo.Value.Oid; // Используем OID как Checksum
                        Debug.WriteLine($"Обновлён URL для LFS файла {fileInfo.FileName}: {lfsDownloadUrl}");
                    }
                    else
                    {
                        Debug.WriteLine($"Не удалось получить URL для LFS файла {fileInfo.FileName}.");
                        // Оставляем fileInfo.DownloadUrl как URL указателя, но помечаем как проблемный, если нужно
                    }
                }
                else
                {
                    Debug.WriteLine($"Не удалось распознать информацию LFS в файле {fileInfo.FileName}.");
                }
            }
        }

        /// <summary>
        /// Извлекает OID и Size из содержимого указателя Git LFS.
        /// </summary>
        /// <param name="content">Содержимое текстового файла указателя.</param>
        /// <returns>Кортеж OID и Size или null, если не удалось извлечь.</returns>
        private static (string Oid, int Size)? ParseLfsPointerContent(string content)
        {
            string? oid = null;
            int size = 0;

            using StringReader reader = new(content);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith(GitHubConstants.LfsOidPrefix))
                {
                    oid = line[GitHubConstants.LfsOidPrefix.Length..];
                }
                else if (line.StartsWith(GitHubConstants.LfsSizePrefix) && int.TryParse(line[GitHubConstants.LfsSizePrefix.Length..], out int parsedSize))
                {
                    size = parsedSize;
                }
            }

            if (!string.IsNullOrEmpty(oid))
            {
                Debug.WriteLine($"OID: {oid}; Size: {size}");
                return (oid, size);
            }

            return null; // Не удалось извлечь необходимую информацию
        }

        /// <summary>
        /// Асинхронно получает URL для скачивания содержимого файла из Git LFS API.
        /// </summary>
        /// <param name="oid">OID файла в LFS.</param>
        /// <param name="size">Размер файла в байтах.</param>
        /// <returns>URL для скачивания или null в случае ошибки.</returns>
        private static async Task<string?> GetLfsDownloadUrlAsync(string oid, int size)
        {
            // Шаг 1: Подготовка данных запроса
            string batchUrl = $"https://github.com/{GitHubConstants.RepositoryOwner}/{GitHubConstants.RepositoryName}.git/info/lfs/objects/batch";
            string jsonRequest = $$"""
        {
          "operation": "download",
          "transfer": ["basic"],
          "objects": [
            {
              "oid": "{{oid}}",
              "size": {{size}}
            }
          ]
        }
        """;

            using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            // Content-Type уже установлен автоматически

            using var request = new HttpRequestMessage(HttpMethod.Post, batchUrl);
            request.Headers.Add("Accept", "application/vnd.git-lfs+json");
            request.Content = content;

            try
            {
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseJson = await response.Content.ReadAsStringAsync();

                // Шаг 2: Вызов вспомогательного метода для разбора JSON
                var downloadResult = ParseLfsDownloadResponse(responseJson);
                if (downloadResult != null)
                {
                    Debug.WriteLine($"Получена временная ссылка: {downloadResult.Value.href}");
                    return downloadResult.Value.href;
                }
                else
                {
                    Debug.WriteLine("Не удалось извлечь URL для скачивания из ответа LFS API.");
                    return null;
                }
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine($"Ошибка HTTP при запросе к API LFS: {e.Message}");
                return null;
            }
            catch (JsonException e) // Уточнённый тип исключения для JSON
            {
                Debug.WriteLine($"Ошибка разбора JSON от LFS API: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Произошла ошибка: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Разбирает JSON-ответ от LFS API для извлечения URL скачивания.
        /// </summary>
        /// <param name="json">JSON-строка ответа.</param>
        /// <returns>Кортеж href и headers или null.</returns>
        private static (string href, Dictionary<string, string>? headers)? ParseLfsDownloadResponse(string json)
        {
            using var jsonDoc = JsonDocument.Parse(json);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("objects", out var objectsArray) || objectsArray.GetArrayLength() == 0)
            {
                Debug.WriteLine("Ответ LFS не содержит массив 'objects' или он пуст.");
                return null;
            }

            var firstObject = objectsArray[0];
            if (!firstObject.TryGetProperty("actions", out var actionsObj) || !actionsObj.TryGetProperty("download", out var downloadAction))
            {
                Debug.WriteLine("Поле 'actions' или 'download' не найдено в ответе для объекта.");
                return null;
            }

            if (!downloadAction.TryGetProperty("href", out var hrefElement))
            {
                Debug.WriteLine("Поле 'href' не найдено в действии 'download'.");
                return null;
            }

            string? href = hrefElement.GetString();
            if (string.IsNullOrEmpty(href))
            {
                Debug.WriteLine("Полученное значение 'href' пусто.");
                return null;
            }

            Dictionary<string, string>? headers = null;
            if (downloadAction.TryGetProperty("header", out var headerElement))
            {
                headers = new Dictionary<string, string>();
                foreach (var property in headerElement.EnumerateObject())
                {
                    headers[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }

            return (href, headers);
        }
    }
}