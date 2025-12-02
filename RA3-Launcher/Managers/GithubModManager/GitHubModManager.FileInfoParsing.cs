// Managers.Github/GitHubModManager.FileInfoParsing.cs
using Items.Mod;
using Newtonsoft.Json;
using RA3_Launcher.Managers.GithubModManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace Managers.Github
{
    public static partial class GitHubModManager
    {
        /// <summary>
        /// Разбирает содержимое файла ModInfo.txt и заполняет соответствующие поля объекта <see cref="Mod"/>.
        /// </summary>
        /// <param name="mod">Объект мода для обновления.</param>
        /// <param name="content">Содержимое файла.</param>
        private static void ParseInfoFile(Mod mod, string content)
        {
            using StringReader reader = new(content);
            string? line;
            string? currentKey = null;
            StringBuilder? currentValueBuilder = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(['='], 2);
                if (parts.Length == 2)
                {
                    ProcessCurrentKeyValue(mod, currentKey, currentValueBuilder);
                    currentKey = parts[0].Trim().ToLower();
                    currentValueBuilder = new StringBuilder(parts[1]);
                }
                else
                {
                    if (currentKey != null && currentValueBuilder != null)
                    {
                        currentValueBuilder.AppendLine(line);
                    }
                }
            }

            ProcessCurrentKeyValue(mod, currentKey, currentValueBuilder);
        }

        /// <summary>
        /// Разбирает содержимое файла VersionInfo.txt и заполняет соответствующие поля объекта <see cref="ModVersion"/>.
        /// </summary>
        /// <param name="version">Объект версии для обновления.</param>
        /// <param name="content">Содержимое файла.</param>
        private static void ParseVersionInfoFile(ModVersion version, string content)
        {
            using StringReader reader = new(content);
            string? line;
            string? currentKey = null;
            StringBuilder? currentValueBuilder = null;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split(['='], 2);
                if (parts.Length == 2)
                {
                    ProcessCurrentKeyValue(version, currentKey, currentValueBuilder);
                    currentKey = parts[0].Trim().ToLower();
                    currentValueBuilder = new StringBuilder(parts[1]);
                }
                else
                {
                    if (currentKey != null && currentValueBuilder != null)
                    {
                        currentValueBuilder.AppendLine(line);
                    }
                }
            }

            ProcessCurrentKeyValue(version, currentKey, currentValueBuilder);
        }

        /// <summary>
        /// Обрабатывает текущую пару ключ-значение для объекта <see cref="Mod"/>.
        /// </summary>
        /// <param name="mod">Объект мода.</param>
        /// <param name="currentKey">Текущий ключ.</param>
        /// <param name="currentValueBuilder">Построитель текущего значения.</param>
        private static void ProcessCurrentKeyValue(Mod mod, string? currentKey, StringBuilder? currentValueBuilder)
        {
            if (currentKey != null && currentValueBuilder != null)
            {
                string value = currentValueBuilder.ToString();
                AssignParsedValue(mod, currentKey, value);
            }
        }

        /// <summary>
        /// Обрабатывает текущую пару ключ-значение для объекта <see cref="ModVersion"/>.
        /// </summary>
        /// <param name="version">Объект версии.</param>
        /// <param name="currentKey">Текущий ключ.</param>
        /// <param name="currentValueBuilder">Построитель текущего значения.</param>
        private static void ProcessCurrentKeyValue(ModVersion version, string? currentKey, StringBuilder? currentValueBuilder)
        {
            if (currentKey != null && currentValueBuilder != null)
            {
                string value = currentValueBuilder.ToString();
                AssignParsedValue(version, currentKey, value);
            }
        }

        /// <summary>
        /// Назначает значение свойству объекта <see cref="Mod"/> по ключу.
        /// </summary>
        /// <param name="mod">Объект мода.</param>
        /// <param name="key">Ключ из файла.</param>
        /// <param name="value">Значение из файла.</param>
        private static void AssignParsedValue(Mod mod, string key, string value)
        {
            switch (key)
            {
                case GitHubConstants.DescriptionKey:
                    mod.Description = value;
                    break;
                case GitHubConstants.CreationDateKey:
                    if (DateTime.TryParseExact(value, GitHubConstants.DateTimeRA3Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime creationDate))
                    {
                        mod.CreationDate = creationDate;
                    }
                    break;
                case GitHubConstants.AuthorKey:
                    mod.Author = value;
                    break;
                case GitHubConstants.CategoryKey:
                case GitHubConstants.CategoriesKey:
                    mod.Category = value;
                    break;
                case GitHubConstants.GameVersionKey:
                    mod.GameVersion = value;
                    break;
                // Новые case для дополнительных полей
                case GitHubConstants.WebsiteKey:
                    mod.Website = value;
                    break;
                case GitHubConstants.RepositoryUrlKey:
                    mod.RepositoryUrl = value;
                    break;
                case GitHubConstants.IsFeaturedKey:
                    if (bool.TryParse(value, out bool isFeatured))
                    {
                        mod.IsFeatured = isFeatured;
                    }
                    break;
                case GitHubConstants.TotalDownloadsKey:
                    if (long.TryParse(value, out long totalDownloads))
                    {
                        mod.TotalDownloads = totalDownloads;
                    }
                    break;
            }
        }

        /// <summary>
        /// Назначает значение свойству объекта <see cref="ModVersion"/> по ключу.
        /// </summary>
        /// <param name="version">Объект версии.</param>
        /// <param name="key">Ключ из файла.</param>
        /// <param name="value">Значение из файла.</param>
        private static void AssignParsedValue(ModVersion version, string key, string value)
        {
            switch (key)
            {
                case GitHubConstants.ChangelogKey:
                    version.Changelog = value;
                    break;
                case GitHubConstants.UpdationDateKey:
                case GitHubConstants.UpdateDateKey:
                    if (DateTime.TryParseExact(value, GitHubConstants.DateTimeRA3Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime updateDate))
                    {
                        version.UpdateDate = updateDate;
                    }
                    break;
                // Новый case для RequiredGameVersion в VersionInfo.txt
                case GitHubConstants.RequiredGameVersionKey: // Используем ту же константу, но обрабатываем в Version
                    version.RequiredGameVersion = value;
                    break;
            }
        }
    }
}