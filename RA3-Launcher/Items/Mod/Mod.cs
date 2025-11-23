using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Items.Mod
{
    public class Mod
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


        [JsonPropertyName("dependencies")]
        [JsonProperty("dependencies")]
        public List<string> Dependencies { get; set; } = [];


        [JsonPropertyName("downloadCount")]
        [JsonProperty("downloadCount")]
        public int DownloadCount { get; set; }


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
    }
}
