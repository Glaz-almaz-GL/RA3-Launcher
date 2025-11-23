using Items;
using Managers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RA3_Launcher.Managers
{
    public static partial class ModManager
    {
        public static List<ModInfo> GetModsFromDocuments()
        {
            List<ModInfo> mods = [];

            if (Directory.Exists(FilePaths.ModsDirPath))
            {
                foreach (string modPath in Directory.GetDirectories(FilePaths.ModsDirPath))
                {
                    string? skudefFile = Directory.GetFiles(modPath, "*.skudef").FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(skudefFile))
                    {
                        // Path.GetFileNameWithoutExtension возвращает "RemixEn_0.3.7"
                        (string Name, string Version) = ParseFileName(Path.GetFileNameWithoutExtension(skudefFile));
                        string modName = Name;
                        string modVersion = Version;

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
            Regex regex = ModInfoRegex();
            Match match = regex.Match(fileNameWithoutExtension);

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
