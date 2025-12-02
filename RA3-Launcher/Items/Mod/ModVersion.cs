// Items.Mod/ModVersion.cs
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Items.Mod
{
    /// <summary>
    /// Информация о версии мода.
    /// </summary>
    public class ModVersion
    {
        [JsonPropertyName("versionNumber")]
        [JsonProperty("versionNumber")]
        public string? VersionNumber { get; set; }

        [JsonPropertyName("changelog")]
        [JsonProperty("changelog")]
        public string? Changelog { get; set; }

        [JsonPropertyName("updateDate")]
        [JsonProperty("updateDate")]
        public DateTime UpdateDate { get; set; }

        [JsonPropertyName("isBeta")]
        [JsonProperty("isBeta")]
        public bool IsBeta { get; set; } = false;

        [JsonPropertyName("requiredGameVersion")]
        [JsonProperty("requiredGameVersion")]
        public string? RequiredGameVersion { get; set; }

        [JsonPropertyName("availableLanguages")]
        [JsonProperty("availableLanguages")]
        public List<string> AvailableLanguages { get; set; } = [];

        /// <summary>
        /// Словарь всех файлов версии (ключ - имя файла).
        /// </summary>
        [JsonPropertyName("allFiles")]
        [JsonProperty("allFiles")]
        public Dictionary<string, ModFileInfo> AllFiles { get; set; } = [];

        /// <summary>
        /// Список основных файлов версии (файлы .big/.lyi из каталога версии, не из Languages).
        /// </summary>
        [JsonPropertyName("versionMainFiles")]
        [JsonProperty("versionMainFiles")]
        public List<ModFileInfo> VersionMainFiles { get; set; } = [];
    }
}