using System;
using System.Diagnostics;
using System.IO;

namespace Managers
{
    public static class RegistryManager
    {
        public static void FixRegistry()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                ApplyRegFile(FilePaths.Fix64RegistryPath);
            }
            else
            {
                ApplyRegFile(FilePaths.Fix32RegistryPath);
            }
        }

        private static void ApplyRegFile(string? regFilePath)
        {
            if (!File.Exists(regFilePath))
            {
                Debug.WriteLine($"Файл реестра не найден: {regFilePath}");
                return;
            }

            try
            {
                // Используем ProcessStartInfo для безопасного запуска
                ProcessStartInfo startInfo = new()
                {
                    FileName = "regedit.exe",
                    Arguments = $"/s \"{regFilePath}\"", // /s - подавляет запросы подтверждения
                    UseShellExecute = false, // Позволяет перенаправить потоки, но для regedit необязательно
                    CreateNoWindow = true, // Скрывает окно regedit
                    Verb = "runas" // Запускает с правами администратора (вызовет UAC)
                };

                using Process? process = Process.Start(startInfo);

                if (process != null)
                {
                    process.WaitForExit(); // Ждем завершения выполнения regedit

                    if (process.ExitCode == 0)
                    {
                        GrowlsManager.ShowSuccesMsg("Файл реестра успешно применен.");
                    }
                    else
                    {
                        GrowlsManager.ShowErrorMsg($"Ошибка при применении файла реестра. Код выхода: {process.ExitCode}");
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                // Возникает, если процесс не найден или пользователь отказался от UAC
                GrowlsManager.ShowErrorMsg($"Ошибка запуска regedit: {ex.Message}");
            }
            catch (Exception ex)
            {
                GrowlsManager.ShowErrorMsg($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
