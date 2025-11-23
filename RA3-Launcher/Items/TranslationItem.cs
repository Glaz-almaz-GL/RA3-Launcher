using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Items
{
    public class TranslationItem
    {

        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;


        [JsonPropertyName("version")]
        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;


        [JsonPropertyName("description")]
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;


        [JsonPropertyName("creationDate")]
        [JsonProperty("creationDate")]
        public DateTime CreationDate { get; set; } = DateTime.MinValue;


        [JsonPropertyName("language")]
        [JsonProperty("languages")]
        public IEnumerable<string> Languages { get; set; } = [];


        [JsonPropertyName("torrentUrl")]
        [JsonProperty("torrentUrl")]
        public string TorrentUrl { get; set; } = string.Empty;


        [JsonPropertyName("isDownlodable")]
        [JsonProperty("isDownlodable")]
        public bool IsDownlodable { get; set; } = true;

        public TranslationItem(string name, string version, string description, DateTime creationDate, IEnumerable<string> languages, string torrentUrl, bool isDownlodable)
        {
            Name = name;
            Version = version;
            Description = description;
            CreationDate = creationDate;
            Languages = languages;
            TorrentUrl = torrentUrl;
            IsDownlodable = isDownlodable;
        }

        public TranslationItem() { }
    }
}
