using Items;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Managers
{
    public static class SettingsManager
    {
        private const string ErrorTitle = "Error";

        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RA3_Launcher", // Подкаталог для вашего приложения
            "settings.json" // Имя файла настроек
        );

        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        [JsonIgnore]
        public static SettingsItem CurrentSettings { get; } = LoadSettings();

        private static SettingsItem LoadSettings()
        {
            Trace.WriteLine($"[SettingsManager] Попытка загрузки настроек из: {SettingsFilePath}", "Info");

            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(SettingsFilePath);
                    Trace.WriteLine($"[SettingsManager] Файл настроек найден, размер: {jsonString.Length} символов", "Debug");

                    SettingsItem? settings = JsonSerializer.Deserialize<SettingsItem>(jsonString, jsonSerializerOptions);

                    if (settings != null)
                    {
                        Trace.WriteLine($"[SettingsManager] Настройки успешно загружены из файла: {SettingsFilePath}", "Info");
                        return settings;
                    }
                    else
                    {
                        Trace.WriteLine("[SettingsManager] Десериализация вернула null, возвращаем настройки по умолчанию", "Warning");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SettingsManager] Ошибка при загрузке настроек: {ex.GetType().Name} - {ex.Message}", ErrorTitle);
                    Debug.WriteLine($"[SettingsManager] Stack Debug: {ex.StackTrace}", ErrorTitle);
                    // Возвращаем настройки по умолчанию в случае ошибки
                }
            }
            else
            {
                Debug.WriteLine($"[SettingsManager] Файл настроек не найден: {SettingsFilePath}, создаём настройки по умолчанию", "Info");
            }

            // Если файл не существует или произошла ошибка, возвращаем настройки по умолчанию
            Debug.WriteLine("[SettingsManager] Возвращаем настройки по умолчанию", "Info");
            return new SettingsItem();
        }

        public static void SaveSettings(SettingsItem settings)
        {
            Debug.WriteLine($"[SettingsManager] Попытка сохранения настроек в: {SettingsFilePath}", "Info");

            try
            {
                // Создаём директорию, если она не существует
                string? directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.WriteLine($"[SettingsManager] Создана директория: {directory}", "Info");
                }

                string jsonString = JsonSerializer.Serialize(settings, jsonSerializerOptions);
                Debug.WriteLine($"[SettingsManager] Сериализованные настройки, размер: {jsonString.Length} символов", "Debug");

                File.WriteAllText(SettingsFilePath, jsonString);
                Debug.WriteLine($"[SettingsManager] Настройки успешно сохранены в: {SettingsFilePath}", "Info");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SettingsManager] Ошибка при сохранении настроек: {ex.GetType().Name} - {ex.Message}", ErrorTitle);
                Debug.WriteLine($"[SettingsManager] Stack Trace: {ex.StackTrace}", ErrorTitle);
                throw; // Перебрасываем исключение, если вызывающий код должен обработать его
            }
        }

        // Метод для сохранения текущих настроек
        public static void SaveCurrentSettings()
        {
            Debug.WriteLine("[SettingsManager] Вызван метод SaveCurrentSettings", "Debug");
            SaveSettings(CurrentSettings);
        }
    }
}