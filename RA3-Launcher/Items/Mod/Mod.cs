// Items.Mod/Mod.cs
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Items.Mod
{
    /// <summary>
    /// Расширенная информация о моде.
    /// </summary>
    public partial class Mod
    {
        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        [JsonProperty("description")]
        public string? Description { get; set; } = string.Empty;

        [JsonPropertyName("creationDate")]
        [JsonProperty("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonPropertyName("author")]
        [JsonProperty("author")]
        public string? Author { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        [JsonProperty("category")]
        public string? Category { get; set; } = string.Empty;

        [JsonPropertyName("gameVersion")]
        [JsonProperty("gameVersion")]
        public string? GameVersion { get; set; } = string.Empty;

        // Новые поля
        [JsonPropertyName("website")]
        [JsonProperty("website")]
        public string? Website { get; set; } = null;

        [JsonPropertyName("repositoryUrl")]
        [JsonProperty("repositoryUrl")]
        public string? RepositoryUrl { get; set; } = null;

        [JsonPropertyName("screenshots")]
        [JsonProperty("screenshots")]
        public List<string> Screenshots { get; set; } = [];

        [JsonPropertyName("totalDownloads")]
        [JsonProperty("totalDownloads")]
        public long TotalDownloads { get; set; } = 0;

        [JsonPropertyName("lastUpdated")]
        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [JsonPropertyName("latestVersion")]
        [JsonProperty("latestVersion")]
        public string? LatestVersion { get; set; } = string.Empty;

        [JsonPropertyName("availableLanguages")]
        [JsonProperty("availableLanguages")]
        public List<string> AvailableLanguages { get; set; } = [];

        [JsonPropertyName("versions")]
        [JsonProperty("versions")]
        public List<ModVersion> Versions { get; set; } = [];

        [JsonPropertyName("isFeatured")]
        [JsonProperty("isFeatured")]
        public bool IsFeatured { get; set; } = false;

        /// <summary>
        /// Список основных файлов мода (файлы .big/.lyi из корня каталога мода).
        /// </summary>
        [JsonPropertyName("mainFiles")]
        [JsonProperty("mainFiles")]
        public List<ModFileInfo> MainFiles { get; set; } = [];

        [RelayCommand]
        public async Task Download()
        {

        }

        [RelayCommand]
        public async Task Delete()
        {

        }
    }
}