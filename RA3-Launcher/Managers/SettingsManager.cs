using RA3_Launcher.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RA3_Launcher.Managers
{
    public static class SettingsManager
    {
        [JsonIgnore]
        public static SettingsItem CurrentSettings { get; } = LoadSettings();
        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RA3_Launcher", // Подкаталог для вашего приложения
            "settings.json" // Имя файла настроек
        );

        private static SettingsItem LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<SettingsItem>(jsonString, jsonSerializerOptions);

                    if (settings != null)
                    {
                        return settings;
                    }
                }
                catch (Exception ex)
                {
                    // Логирование ошибки (если используется логгер) или вывод в консоль
                    Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
                    // Возвращаем настройки по умолчанию в случае ошибки
                }
            }

            // Если файл не существует или произошла ошибка, возвращаем настройки по умолчанию
            return new SettingsItem();
        }

        public static void SaveSettings(SettingsItem settings)
        {
            try
            {
                // Создаём директорию, если она не существует
                var directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true // Форматирует JSON для лучшей читаемости
                });

                File.WriteAllText(SettingsFilePath, jsonString);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
                throw; // Перебрасываем исключение, если вызывающий код должен обработать его
            }
        }

        // Метод для сохранения текущих настроек
        public static void SaveCurrentSettings()
        {
            SaveSettings(CurrentSettings);
        }
    }
}
