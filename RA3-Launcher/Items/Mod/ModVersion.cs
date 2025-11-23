using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Items.Mod
{
    public class ModVersion
    {
        [JsonPropertyName("versionNumber")]
        [JsonProperty("versionNumber")]
        public string? VersionNumber { get; set; } = string.Empty;

        [JsonPropertyName("changelog")]
        [JsonProperty("changelog")]
        public string? Changelog { get; set; } = string.Empty;

        [JsonPropertyName("updateDate")]
        [JsonProperty("updateDate")]
        public DateTime UpdateDate { get; set; }

        [JsonPropertyName("modSize")]
        [JsonProperty("modSize")]
        public long ModSize { get; set; }

        [JsonPropertyName("downloadUrl")]
        [JsonProperty("downloadUrl")]
        public string? DownloadUrl { get; set; } = string.Empty;

        [JsonPropertyName("checksum")]
        [JsonProperty("checksum")]
        public string? Checksum { get; set; } = string.Empty;

        [JsonPropertyName("isBeta")]
        [JsonProperty("isBeta")]
        public bool IsBeta { get; set; }

        [JsonPropertyName("requiredGameVersion")]
        [JsonProperty("requiredGameVersion")]
        public string? RequiredGameVersion { get; set; } = string.Empty;

        [JsonPropertyName("availableLanguages")]
        [JsonProperty("availableLanguages")]
        public List<string> AvailableLanguages { get; set; } = [];

        [JsonPropertyName("languageFiles")]
        [JsonProperty("languageFiles")]
        public Dictionary<string, string> LanguageFiles { get; set; } = [];

        // Основной файл мода, специфичный для этой версии (может отсутствовать)
        [JsonPropertyName("mainModFile")]
        [JsonProperty("mainModFile")]
        public string? MainModFile { get; set; } = string.Empty;

        // Список общих файлов мода, найденных в корне мода (для всех версий)
        [JsonPropertyName("commonModFiles")]
        [JsonProperty("commonModFiles")]
        public List<string> CommonModFiles { get; set; } = [];
    }
}