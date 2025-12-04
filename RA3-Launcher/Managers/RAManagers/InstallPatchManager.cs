using Managers.AvaloniaManagers;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Text;

namespace Managers.RAManagers
{
    public static class InstallPatchManager
    {
        public static void Install4GBPatch()
        {
            try
            {
                ProcessStartInfo fourGbPatchInfo = new()
                {
                    FileName = FilePaths.FourGBPatchPath,
                    Arguments = "/S",
                    CreateNoWindow = true, // Скрывает окно
                    Verb = "runas" // Запускает с правами администратора (вызовет UAC)
                };

                using Process? process = Process.Start(fourGbPatchInfo);

                if (process != null)
                {
                    process.WaitForExit(); // Ждем завершения выполнения regedit

                    if (process.ExitCode == 0)
                    {
                        GrowlsManager.ShowSuccessMsg("4GB Патч применён успешно применен.");
                    }
                    else
                    {
                        GrowlsManager.ShowErrorMsg($"Ошибка при применении 4GB патча. Код выхода: {process.ExitCode}");
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                if (ex.Message.Contains("требует повышения"))
                {
                    GrowlsManager.ShowErrorMsg("Не удалось установить патч, требуется запуск от имени администратора");
                    return;
                }

                // Возникает, если процесс не найден или пользователь отказался от UAC
                GrowlsManager.ShowErrorMsg($"Ошибка запуска 4GB-Patch: {ex.Message}");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Требует повышения"))
                {
                    GrowlsManager.ShowErrorMsg("Не удалось установить патч, требуется запуск от имени администратора");
                    return;
                }

                GrowlsManager.ShowErrorMsg($"Произошла ошибка: {ex.Message}");
            }
        }

        public static bool ApplyCDKey(string cdKey)
        {
            if (OperatingSystem.IsWindows())
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Electronic Arts\Electronic Arts\Red Alert 3\ergc", writable: true);
                if (key != null)
                {
                    // Изменяем значение по умолчанию (Default Value) - это значение с именем "(Default)" в редакторе реестра
                    key.SetValue("", cdKey);

                    // Изменяем значение с именем "(Default)" (именованное значение)
                    key.SetValue("Default", cdKey);

                    return true;
                }
                else
                {
                    // Ключ не найден
                    GrowlsManager.ShowWarningMsg("Ключ реестра не найден.");
                }
            }
            else
            {
                GrowlsManager.ShowWarningMsg("Операции с реестром Windows недоступны на этой платформе.");
            }

            return false;
        }

        public static string GenerateCDKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new();
            StringBuilder result = new();

            for (int i = 0; i < 20; i++) // 20 символов без разделителей
            {
                result.Append(chars[random.Next(chars.Length)]);
                if ((i + 1) % 4 == 0 && i != 19) // Добавляем "-" после каждых 4 символов
                {
                    result.Append('-');
                }
            }

            return result.ToString();
        }
    }
}
