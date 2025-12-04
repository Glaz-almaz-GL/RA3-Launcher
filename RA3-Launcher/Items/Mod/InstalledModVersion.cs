using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Items.Mod
{
    public class InstalledModVersion(string version, string installPath)
    {
        public string Version { get; } = version;
        public string InstallPath { get; } = installPath;
        public DateTime InstallDate { get; } = Directory.Exists(installPath)
                ? Directory.GetCreationTime(installPath)
                : DateTime.Now;

        public bool IsLanguageInstalled(string languageName)
        {
            if (!Directory.Exists(InstallPath))
            {
                return false;
            }

            string bigPath = Path.Combine(InstallPath, $"{languageName}.big");
            string lyiPath = Path.Combine(InstallPath, $"{languageName}.lyi");
            return File.Exists(bigPath) || File.Exists(lyiPath);
        }

        public List<string> GetInstalledLanguages()
        {
            if (!Directory.Exists(InstallPath))
            {
                return [];
            }

            List<string> installedLanguages = [];

            // Ищем все .big и .lyi файлы в InstallPath
            string[] bigFiles = Directory.GetFiles(InstallPath, "*.big", SearchOption.TopDirectoryOnly);
            string[] lyiFiles = Directory.GetFiles(InstallPath, "*.lyi", SearchOption.TopDirectoryOnly);

            // Извлекаем имена файлов без расширений и объединяем
            IEnumerable<string> languageNames = bigFiles.Select(f => Path.GetFileNameWithoutExtension(f))
                                        .Concat(lyiFiles.Select(f => Path.GetFileNameWithoutExtension(f)))
                                        .Distinct(StringComparer.OrdinalIgnoreCase);

            installedLanguages.AddRange(languageNames);
            return installedLanguages;
        }
    }
}
