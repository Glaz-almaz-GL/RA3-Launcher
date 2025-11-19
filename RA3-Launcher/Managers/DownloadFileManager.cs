using Huskui.Avalonia.Controls;
using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Managers
{
    public static partial class DownloadFileManager
    {
        private static readonly HttpClient _httpClient = new();
        private static ClientEngine? _engine = null;
        private static TorrentManager? _manager = null;

        /// <summary>
        /// Скачивает файлы из .torrent файла.
        /// </summary>
        /// <param name="torrentPath">Путь к .torrent файлу.</param>
        /// <param name="downloadDirectory">Папка, куда будут скачаны файлы.</param>
        /// <returns>True, если загрузка успешно завершена (или остановлена пользователем), false в случае ошибки.</returns>
        public static async Task<bool> DownloadTorrentFiles(string torrentPath, string downloadDirectory)
        {
            if (!File.Exists(torrentPath))
            {
                Debug.WriteLine($"Файл .torrent не найден: {torrentPath}");
                return false;
            }

            downloadDirectory = Path.Combine(FilePaths.DownloadFilesDirPath, downloadDirectory);

            if (!Directory.Exists(downloadDirectory))
            {
                try
                {
                    Directory.CreateDirectory(downloadDirectory);
                    Debug.WriteLine($"Создана папка для загрузки: {downloadDirectory}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Не удалось создать папку для загрузки: {ex.Message}");
                    return false;
                }
            }

            try
            {
                // Настройка клиента
                EngineSettingsBuilder engineSettings = new()
                {
                    AllowPortForwarding = true,
                    AutoSaveLoadDhtCache = true,
                };

                _engine = new ClientEngine(engineSettings.ToSettings());

                // Загружаем .torrent файл
                Torrent torrent = await Torrent.LoadAsync(torrentPath);

                // Создаём торрент-манагер через AddAsync - Правильный способ!
                _manager = await _engine.AddAsync(torrent, downloadDirectory, new TorrentSettings());

                GrowlItem? downloadGrowl = GrowlsManager.ShowProgressInfoMsg("Скачивание RA3 BattleNet", progress: _manager.Progress / 1000.0);

                // Подписываемся на событие изменения прогресса (опционально)
                _manager.PieceHashed += async (o, e) => await UpdateProgress(downloadGrowl); // Прогресс от 0 до 1000 (0.0% - 100.0%)

                Debug.WriteLine($"Начата загрузка торрента: {torrent.Name}");
                Debug.WriteLine("Нажмите 'q' для остановки.");

                // Останавливаем и удаляем торрент-манагер
                await _manager.StopAsync();

                Debug.WriteLine("Загрузка остановлена.");
                return true; // Успешно завершено (по команде пользователя)
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Произошла ошибка: {ex.Message}");
                return false; // Ошибка
            }
            finally
            {
                _engine?.Dispose(); // Освобождаем ресурсы
            }
        }

        private static async Task UpdateProgress(GrowlItem? downloadGrowl)
        {
            if (downloadGrowl != null && _manager != null)
            {
                downloadGrowl.Progress = _manager.Progress / 1000.0;
            }
            else if (downloadGrowl == null && (_manager?.State == TorrentState.Downloading || _manager?.State == TorrentState.Seeding))
            {
                await _manager.StopAsync();
                GrowlsManager.ShowInfoMsg("Скачивание файла было отменено");
            }
        }

        /// <summary>
        /// Асинхронно скачивает файл по указанному URL и сохраняет его в заданный путь.
        /// </summary>
        /// <param name="destinationPath">Путь к файлу, куда будет сохранён скачанный файл (например, "myfile.exe").</param>
        /// <returns>True, если файл успешно скачан, иначе False.</returns>
        public static async Task<string> DownloadFileAsync(string baseUrl, string relativePath, string destinationPath = "")
        {
            try
            {
                // Объединяем baseUrl и relativePath в абсолютный URL
                Uri absoluteUri = new(new Uri(baseUrl), relativePath);

                destinationPath = Path.Combine(FilePaths.DownloadFilesDirPath, destinationPath);

                Directory.CreateDirectory(FilePaths.DownloadFilesDirPath);

                Debug.WriteLine($"Загрузка файла с {absoluteUri} в {destinationPath}...");

                // Отправляем GET-запрос и получаем HttpResponseMessage
                using (HttpResponseMessage response = await _httpClient.GetAsync(absoluteUri))
                {
                    // Проверяем, был ли запрос успешным (статус 2xx)
                    response.EnsureSuccessStatusCode();

                    // Получаем поток содержимого ответа
                    await using Stream contentStream = await response.Content.ReadAsStreamAsync();
                    // Создаем или перезаписываем файл по указанному пути
                    await using FileStream fileStream = new(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    // Копируем данные из потока ответа в файловый поток
                    await contentStream.CopyToAsync(fileStream);
                }

                Debug.WriteLine("Файл успешно загружен.");
                return destinationPath;
            }
            catch (HttpRequestException httpEx)
            {
                // Ошибка на уровне HTTP (например, 404 Not Found, 500 Internal Server Error)
                Debug.WriteLine($"Ошибка HTTP при загрузке: {httpEx.Message}");
            }
            catch (IOException ioEx)
            {
                // Ошибка при работе с файлом (например, нет прав на запись, диск полон)
                Debug.WriteLine($"Ошибка при записи файла: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                // Любая другая ошибка
                Debug.WriteLine($"Произошла ошибка: {ex.Message}");
                // Рассмотрите возможность логирования стека вызовов: ex.StackTrace
                Debug.WriteLine($"Дополнительно: {ex.StackTrace}");
            }

            return string.Empty; // В случае ошибки возвращаем пустую строку
        }
    }
}
