// Items.Mod/ModFileInfo.cs
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Items.Mod
{
    /// <summary>
    /// Информация о файле мода.
    /// </summary>
    public class ModFileInfo
    {
        [JsonPropertyName("fileName")]
        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("downloadUrl")]
        [JsonProperty("downloadUrl")]
        public string? DownloadUrl { get; set; }

        [JsonPropertyName("size")]
        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonPropertyName("checksum")]
        [JsonProperty("checksum")]
        public string? Checksum { get; set; }

        [JsonPropertyName("fileType")]
        [JsonProperty("fileType")]
        public ModFileType FileType { get; set; }

        /// <summary>
        /// Помечает файл как общий (например, файл локализации из подкаталога Languages версии).
        /// Также может помечать файл из корня мода, добавленный в контексте версии.
        /// </summary>
        [JsonPropertyName("isCommonFile")]
        [JsonProperty("isCommonFile")]
        public bool IsCommonFile { get; set; } = false;

        /// <summary>
        /// Помечает файл как основной файл мода (находится в корне каталога мода).
        /// </summary>
        [JsonPropertyName("isModMainFile")]
        [JsonProperty("isModMainFile")]
        public bool IsModMainFile { get; set; } = false;

        /// <summary>
        /// Помечает файл как основной файл версии (находится в каталоге версии, не в Languages).
        /// </summary>
        [JsonPropertyName("isVersionMainFile")]
        [JsonProperty("isVersionMainFile")]
        public bool IsVersionMainFile { get; set; } = false;

        [JsonPropertyName("languageCode")]
        [JsonProperty("languageCode")]
        public string? LanguageCode { get; set; }

        [JsonPropertyName("lastModified")]
        [JsonProperty("lastModified")]
        public DateTime LastModified { get; set; }
    }
}