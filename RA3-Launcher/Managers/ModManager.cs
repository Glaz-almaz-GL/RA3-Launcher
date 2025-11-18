using RA3_Launcher.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RA3_Launcher.Managers
{
    public static partial class ModManager
    {
        private static readonly string documentsDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string modFolderPath = Path.Combine(documentsDirPath, "Red Alert 3", "Mods");

        public static List<ModInfo> GetModsFromDocuments()
        {
            List<ModInfo> mods = [];

            if (Directory.Exists(modFolderPath))
            {
                foreach (var modPath in Directory.GetDirectories(modFolderPath))
                {
                    string? skudefFile = Directory.GetFiles(modPath, "*.skudef").FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(skudefFile))
                    {
                        // Path.GetFileNameWithoutExtension возвращает "RemixEn_0.3.7"
                        var modInfo = ParseFileName(Path.GetFileNameWithoutExtension(skudefFile));
                        string modName = modInfo.Name;
                        string modVersion = modInfo.Version;

                        if (string.IsNullOrWhiteSpace(modName) || string.IsNullOrWhiteSpace(modVersion))
                        {
                            continue;
                        }

                        mods.Add(new(modName, modVersion, skudefFile));
                    }
                }
            }

            return mods;
        }

        private static (string Name, string Version) ParseFileName(string fileNameWithoutExtension)
        {
            // Регулярное выражение теперь ожидает X.Y или X.Y.Z и т.д.
            var regex = ModInfoRegex();
            var match = regex.Match(fileNameWithoutExtension);

            if (match.Success)
            {
                string name = match.Groups[1].Value; // Часть до последнего подчёркивания и версии
                string version = match.Groups[2].Value; // Версия в формате X.Y или X.Y.Z и т.д.
                return (name, version);
            }

            // Если формат не подошёл, возвращаем пустые строки или выбрасываем исключение
            return (string.Empty, string.Empty);
        }

        // Паттерн: любые символы (1+), затем _, затем цифры.цифры (1+ раз, может повторяться)
        [GeneratedRegex(@"^(.+?)_([0-9]+(?:\.[0-9]+)+)$", RegexOptions.IgnoreCase)]
        private static partial Regex ModInfoRegex();
    }
}
