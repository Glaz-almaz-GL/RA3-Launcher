// Managers.Github/GitHubModManager.FileInfoParsing.cs
using Items.Mod;
using Managers.GithubModManager;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Managers.Github
{
    public static partial class GitHubModManager
    {
        /// <summary>
        /// Разбирает содержимое файла ModInfo.txt и заполняет соответствующие поля объекта <see cref="Mod"/>.
        /// </summary>
        /// <param name="mod">Объект мода для обновления.</param>
        /// <param name="content">Содержимое файла.</param>
        private static async Task ParseInfoFileAsync(ModMetadata mod, string content)
        {
            using StringReader reader = new(content);
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(['='], 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                string key = parts[0].Trim().ToLowerInvariant();
                string value = parts[1];

                switch (key)
                {
                    case GitHubConstants.DescriptionKey:
                        mod.Description = value;
                        break;
                    case GitHubConstants.AuthorKey:
                        mod.Author = value;
                        break;
                    case GitHubConstants.CategoryKey:
                    case GitHubConstants.CategoriesKey:
                        mod.Category = value;
                        break;
                    case GitHubConstants.GameVersionKey:
                        mod.GameVersion = value; // ← теперь Game, а не GameVersion
                        break;
                    case GitHubConstants.WebsiteKey:
                        mod.Website = value;
                        break;
                    case GitHubConstants.RepositoryUrlKey:
                        mod.RepositoryUrl = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Разбирает содержимое файла VersionInfo.txt и заполняет соответствующие поля объекта <see cref="ModVersionMetadata"/>.
        /// </summary>
        /// <param name="version">Объект версии для обновления.</param>
        /// <param name="content">Содержимое файла.</param>
        private static void ParseVersionInfoFile(ModVersionMetadata version, string content)
        {
            using StringReader reader = new(content);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(['='], 2);
                if (parts.Length != 2)
                {
                    continue;
                }

                string key = parts[0].Trim().ToLowerInvariant();
                string value = parts[1];

                if (key == GitHubConstants.ChangelogKey)
                {
                    version.Changelog = value;
                }
                else if (key is GitHubConstants.UpdationDateKey or GitHubConstants.UpdateDateKey && DateTime.TryParseExact(value, GitHubConstants.DateTimeRA3Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    version.ReleaseDate = date;
                }
            }
        }
    }
}